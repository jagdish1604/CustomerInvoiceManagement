using CIM.Repository.Interfaces;
using CIM.Repository.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Repository.Implementations
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly string _connectionString;

        public InvoiceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CreateInvoiceAsync(Invoice invoice, List<InvoiceLine> lines)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var transaction = conn.BeginTransaction();

            try
            {
                
                var cmd = new SqlCommand("usp_CreateInvoice", conn, transaction);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@InvoiceNumber", invoice.InvoiceNumber);
                cmd.Parameters.AddWithValue("@CustomerId", invoice.CustomerId);
                cmd.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate);
                cmd.Parameters.AddWithValue("@DueDate", invoice.DueDate);
                cmd.Parameters.AddWithValue("@Terms", invoice.Terms);
                cmd.Parameters.AddWithValue("@TotalAmount", invoice.TotalAmount);

                var invoiceId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

               
                foreach (var line in lines)
                {
                    var lineCmd = new SqlCommand("usp_CreateInvoiceLine", conn, transaction);
                    lineCmd.CommandType = CommandType.StoredProcedure;

                    lineCmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                    lineCmd.Parameters.AddWithValue("@Description", line.Description);
                    lineCmd.Parameters.AddWithValue("@Quantity", line.Quantity);
                    lineCmd.Parameters.AddWithValue("@UnitPrice", line.UnitPrice);
                    lineCmd.Parameters.AddWithValue("@LineTotal", line.LineTotal);

                    await lineCmd.ExecuteNonQueryAsync();
                }

                transaction.Commit();
                return invoiceId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public async Task<IEnumerable<Customer>> GetCustomerDropdownAsync()
        {
            var list = new List<Customer>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("usp_GetCustomerDropdown", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new Customer
                {
                    CustomerId = (int)reader["CustomerId"],
                    Name = reader["Name"].ToString()
                });
            }

            return list;
        }
        public async Task<(IEnumerable<Invoice>, int)> GetInvoicesAsync(string search, int page, int pageSize, string sortBy, string sortDir)
        {
            var list = new List<Invoice>();
            int totalCount = 0;

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("usp_GetInvoices", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Search", (object?)search ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Page", page);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@SortBy", sortBy);
            cmd.Parameters.AddWithValue("@SortDir", sortDir);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new Invoice
                {
                    InvoiceId = (int)reader["InvoiceId"],
                    InvoiceNumber = reader["InvoiceNumber"].ToString(),
                    CustomerId = (int)reader["CustomerId"],
                    TotalAmount = (decimal)reader["TotalAmount"],

                   
                    InvoiceDate = reader["InvoiceDate"] == DBNull.Value
                        ? DateTime.MinValue
                        : (DateTime)reader["InvoiceDate"],

                    DueDate = reader["DueDate"] == DBNull.Value
                        ? DateTime.MinValue
                        : (DateTime)reader["DueDate"],

                    Terms = reader["Terms"] == DBNull.Value
                        ? 0
                        : (int)reader["Terms"],

                    CustomerName = reader["CustomerName"]?.ToString()
                });
            }

            if (await reader.NextResultAsync())
            {
                if (await reader.ReadAsync())
                    totalCount = (int)reader[0];
            }

            return (list, totalCount);
        }
        public async Task<(Invoice, List<InvoiceLine>)> GetInvoiceByIdAsync(int id)
        {
            Invoice invoice = null;
            var lines = new List<InvoiceLine>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("usp_GetInvoiceById", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@InvoiceId", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            // Invoice
            if (await reader.ReadAsync())
            {
                invoice = new Invoice
                {
                    InvoiceId = (int)reader["InvoiceId"],
                    InvoiceNumber = reader["InvoiceNumber"].ToString(),
                    CustomerId = (int)reader["CustomerId"],
                    TotalAmount = (decimal)reader["TotalAmount"],
                     InvoiceDate = reader["InvoiceDate"] == DBNull.Value
    ? DateTime.MinValue
    : (DateTime)reader["InvoiceDate"],

                    DueDate = reader["DueDate"] == DBNull.Value
    ? DateTime.MinValue
    : (DateTime)reader["DueDate"],

                    Terms = reader["Terms"] == DBNull.Value
    ? 0
    : (int)reader["Terms"]
                };
            }

            // Lines
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    lines.Add(new InvoiceLine
                    {
                        Description = reader["Description"].ToString(),
                        Quantity = (int)reader["Quantity"],
                        UnitPrice = (decimal)reader["UnitPrice"],
                        LineTotal = (decimal)reader["LineTotal"]
                    });
                }
            }

            return (invoice, lines);
        }
        public async Task<bool> UpdateInvoiceAsync(Invoice invoice, List<InvoiceLine> lines)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. Update Invoice
                var cmd = new SqlCommand("usp_UpdateInvoice", conn, transaction);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@InvoiceId", invoice.InvoiceId);
                cmd.Parameters.AddWithValue("@CustomerId", invoice.CustomerId);
                cmd.Parameters.AddWithValue("@InvoiceDate", invoice.InvoiceDate);
                cmd.Parameters.AddWithValue("@DueDate", invoice.DueDate);
                cmd.Parameters.AddWithValue("@Terms", invoice.Terms);
                cmd.Parameters.AddWithValue("@TotalAmount", invoice.TotalAmount);

                await cmd.ExecuteNonQueryAsync();

                // 2. Delete old lines
                var deleteCmd = new SqlCommand("usp_DeleteInvoiceLines", conn, transaction);
                deleteCmd.CommandType = CommandType.StoredProcedure;
                deleteCmd.Parameters.AddWithValue("@InvoiceId", invoice.InvoiceId);
                await deleteCmd.ExecuteNonQueryAsync();

                // 3. Insert new lines
                foreach (var line in lines)
                {
                    var lineCmd = new SqlCommand("usp_CreateInvoiceLine", conn, transaction);
                    lineCmd.CommandType = CommandType.StoredProcedure;

                    lineCmd.Parameters.AddWithValue("@InvoiceId", invoice.InvoiceId);
                    lineCmd.Parameters.AddWithValue("@Description", line.Description);
                    lineCmd.Parameters.AddWithValue("@Quantity", line.Quantity);
                    lineCmd.Parameters.AddWithValue("@UnitPrice", line.UnitPrice);
                    lineCmd.Parameters.AddWithValue("@LineTotal", line.LineTotal);

                    await lineCmd.ExecuteNonQueryAsync();
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public async Task<bool> DeleteInvoiceAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("usp_SoftDeleteInvoice", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@InvoiceId", id);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
        public async Task<int> GetNextInvoiceNumberAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("usp_GetNextInvoiceNumber", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            await conn.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
    }

}
