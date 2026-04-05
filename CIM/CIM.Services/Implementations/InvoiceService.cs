using CIM.Repository.Models;
using CIM.Services.DTOs;
using CIM.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Services.Implementations
{
    using CIM.Repository.Interfaces;
    using CIM.Repository.Models;

    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _repository;

        public InvoiceService(IInvoiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateInvoiceAsync(InvoiceDto dto)
        {
          
            var dueDate = dto.InvoiceDate.AddDays(dto.Terms);

            
            var lines = dto.Lines.Select(x => new InvoiceLine
            {
                Description = x.Description,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                LineTotal = x.Quantity * x.UnitPrice
            }).ToList();

           
            var totalAmount = lines.Sum(x => x.LineTotal);


            var nextNumber = await _repository.GetNextInvoiceNumberAsync();
            var invoiceNumber = $"INV-{nextNumber.ToString("D4")}";


            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                CustomerId = dto.CustomerId,
                InvoiceDate = dto.InvoiceDate,
                DueDate = dueDate,
                Terms = dto.Terms,
                TotalAmount = totalAmount
            };

         
            return await _repository.CreateInvoiceAsync(invoice, lines);
        }
        public async Task<IEnumerable<CustomerDto>> GetCustomerDropdownAsync()
        {
            var data = await _repository.GetCustomerDropdownAsync();

            return data.Select(x => new CustomerDto
            {
                CustomerId = x.CustomerId,
                Name = x.Name
            });
        }
        public async Task<PagedResponse<InvoiceDto>> GetInvoicesAsync(
    string? search,
    int page,
    int pageSize,
    string sortBy,
    string sortDir)
        {
            var (data, totalCount) = await _repository.GetInvoicesAsync(
                search, page, pageSize, sortBy, sortDir);

            var result = data.Select(x => new InvoiceDto
            {
                InvoiceId = x.InvoiceId,   
                CustomerId = x.CustomerId,
               
                InvoiceNumber = x.InvoiceNumber,
                InvoiceDate = x.InvoiceDate,
                Terms = x.Terms,
                TotalAmount = x.TotalAmount,
                CustomerName = x.CustomerName
            }).ToList();

            return new PagedResponse<InvoiceDto>
            {
                Data = result,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }
        public async Task<object> GetInvoiceByIdAsync(int id)
        {
            var (invoice, lines) = await _repository.GetInvoiceByIdAsync(id);

            if (invoice == null)
                return null;

            return new
            {
                invoice.InvoiceId,
                invoice.InvoiceNumber,
                invoice.CustomerId,
                invoice.TotalAmount,

                invoice.InvoiceDate,
                invoice.DueDate,
                invoice.Terms,

                Lines = lines
            };
        }
        public async Task<bool> UpdateInvoiceAsync(int id, InvoiceDto dto)
        {
            var dueDate = dto.InvoiceDate.AddDays(dto.Terms);

            var lines = dto.Lines.Select(x => new InvoiceLine
            {
                Description = x.Description,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                LineTotal = x.Quantity * x.UnitPrice
            }).ToList();

            var totalAmount = lines.Sum(x => x.LineTotal);

            var invoice = new Invoice
            {
                InvoiceId = id,
                CustomerId = dto.CustomerId,
                InvoiceDate = dto.InvoiceDate,
                DueDate = dueDate,
                Terms = dto.Terms,
                TotalAmount = totalAmount
            };

            return await _repository.UpdateInvoiceAsync(invoice, lines);
        }
        public async Task<bool> DeleteInvoiceAsync(int id)
        {
            return await _repository.DeleteInvoiceAsync(id);
        }
    }
}
