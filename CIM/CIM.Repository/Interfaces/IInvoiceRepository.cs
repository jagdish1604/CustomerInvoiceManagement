using CIM.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Repository.Interfaces
{
    public interface IInvoiceRepository
    {
    
        Task<int> CreateInvoiceAsync(Invoice invoice, List<InvoiceLine> lines);
        Task<IEnumerable<Customer>> GetCustomerDropdownAsync();

        Task<(IEnumerable<Invoice>, int)> GetInvoicesAsync(string search, int page, int pageSize, string sortBy, string sortDir);
        Task<(Invoice, List<InvoiceLine>)> GetInvoiceByIdAsync(int id);
        Task<bool> UpdateInvoiceAsync(Invoice invoice, List<InvoiceLine> lines);
        Task<bool> DeleteInvoiceAsync(int id);
        Task<int> GetNextInvoiceNumberAsync();
    }
}
