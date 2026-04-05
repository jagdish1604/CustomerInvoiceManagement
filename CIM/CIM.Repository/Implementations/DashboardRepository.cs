using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CIM.Repository.Implementations
{


    public class DashboardRepository
    {
        private readonly string _connectionString;

        public DashboardRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<(int, int, decimal)> GetDashboardAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("usp_GetDashboard", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();

            int totalCustomers = 0;
            int totalInvoices = 0;
            decimal totalAmount = 0;

            if (await reader.ReadAsync())
            {
                totalCustomers = reader.GetInt32(0);
                totalInvoices = reader.GetInt32(1);
                totalAmount = reader.GetDecimal(2);
            }

            return (totalCustomers, totalInvoices, totalAmount);
        }
    }
}
