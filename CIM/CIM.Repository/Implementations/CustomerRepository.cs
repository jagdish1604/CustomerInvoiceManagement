using CIM.Repository.Interfaces;
using CIM.Repository.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Repository.Implementations
{
   
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;

        public CustomerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<int> CreateCustomerAsync(Customer customer)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("usp_CreateCustomer", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Name", customer.Name);
            cmd.Parameters.AddWithValue("@Address", customer.Address);
            cmd.Parameters.AddWithValue("@PhoneNumber", customer.PhoneNumber);
            cmd.Parameters.AddWithValue("@Email", customer.Email);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<(IEnumerable<Customer>, int)> GetCustomersAsync(string search, int page, int pageSize, string sortBy, string sortDir)
        {
            var list = new List<Customer>();
            int totalCount = 0;

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("usp_GetCustomers", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Search", (object?)search ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Page", page);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);
            cmd.Parameters.AddWithValue("@SortBy", sortBy ?? "CreatedAt");
            cmd.Parameters.AddWithValue("@SortDir", sortDir ?? "DESC");

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();

         
            while (await reader.ReadAsync())
            {
                list.Add(new Customer
                {
                    CustomerId = (int)reader["CustomerId"],
                    Name = reader["Name"].ToString(),
                    Address = reader["Address"].ToString(),
                    PhoneNumber = reader["PhoneNumber"].ToString(),
                    Email = reader["Email"].ToString()
                });
            }

          
            if (await reader.NextResultAsync())
            {
                if (await reader.ReadAsync())
                {
                    totalCount = (int)reader[0];
                }
            }

            return (list, totalCount);
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("usp_SoftDeleteCustomer", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@CustomerId", id);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("usp_GetCustomerById", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@CustomerId", id);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Customer
                {
                    CustomerId = (int)reader["CustomerId"],
                    Name = reader["Name"].ToString(),
                    Address = reader["Address"].ToString(),
                    PhoneNumber = reader["PhoneNumber"].ToString(),
                    Email = reader["Email"].ToString()
                };
            }

            return null;
        }
        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("usp_UpdateCustomer", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
            cmd.Parameters.AddWithValue("@Name", customer.Name);
            cmd.Parameters.AddWithValue("@Address", customer.Address);
            cmd.Parameters.AddWithValue("@PhoneNumber", customer.PhoneNumber);
            cmd.Parameters.AddWithValue("@Email", customer.Email);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("usp_CheckCustomerEmailExists", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Email", email);

            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync() > 0;
        }
    }
}
