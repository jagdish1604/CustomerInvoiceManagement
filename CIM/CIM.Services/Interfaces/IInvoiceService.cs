using CIM.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Services.Interfaces
{
    
    public interface IInvoiceService
    {
        Task<int> CreateInvoiceAsync(InvoiceDto dto);


        Task<PagedResponse<InvoiceDto>> GetInvoicesAsync(
     string? search,
     int page,
     int pageSize,
     string sortBy,
     string sortDir
 );

        Task<object> GetInvoiceByIdAsync(int id);

        Task<IEnumerable<CustomerDto>> GetCustomerDropdownAsync();
        Task<bool> UpdateInvoiceAsync(int id, InvoiceDto dto);

        
        Task<bool> DeleteInvoiceAsync(int id);
    }

  
}
