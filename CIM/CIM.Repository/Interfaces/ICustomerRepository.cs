using CIM.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Repository.Interfaces
{
    public interface ICustomerRepository
    {
        Task<int> CreateCustomerAsync(Customer customer);      
        Task<Customer> GetCustomerByIdAsync(int id);
        Task<bool> UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int id);
        Task<(IEnumerable<Customer>, int)> GetCustomersAsync(string search, int page, int pageSize, string sortBy, string sortDir);
        Task<bool> CheckEmailExistsAsync(string email);
    }
}
