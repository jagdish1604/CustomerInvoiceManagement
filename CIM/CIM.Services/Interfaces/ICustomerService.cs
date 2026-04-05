using CIM.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<int> CreateCustomerAsync(CustomerDto dto);
        Task<PagedResponse<CustomerDto>> GetCustomersAsync(string search, int page, int pageSize, string sortBy, string sortDir);
        Task<CustomerDto> GetCustomerByIdAsync(int id);
        Task<bool> UpdateCustomerAsync(CustomerDto dto);
        Task<bool> DeleteCustomerAsync(int id);
    }
}
