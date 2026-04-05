using CIM.Repository.Interfaces;
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
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;

        public CustomerService(ICustomerRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateCustomerAsync(CustomerDto dto)
        {
            var exists = await _repository.CheckEmailExistsAsync(dto.Email);

            if (exists)
                throw new Exception("Email already exists");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new Exception("Customer name is required");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new Exception("Email is required");

            if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                throw new Exception("Phone number is required");

            if (string.IsNullOrWhiteSpace(dto.Address))
                throw new Exception("Address is required");


            var customer = new Customer
            {
                Name = dto.Name,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email
            };

            return await _repository.CreateCustomerAsync(customer);
        }

        public async Task<PagedResponse<CustomerDto>> GetCustomersAsync(string search, int page, int pageSize, string sortBy, string sortDir)
        {
            var (data, totalCount) = await _repository.GetCustomersAsync(search,  page, pageSize , sortBy, sortDir);

            var result = data.Select(x => new CustomerDto
            {
                CustomerId = x.CustomerId,
                Name = x.Name,
                Address = x.Address,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email
            });

            return new PagedResponse<CustomerDto>
            {
                Data = result,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            return await _repository.DeleteCustomerAsync(id);
        }

        public async Task<CustomerDto> GetCustomerByIdAsync(int id)
        {
            var data = await _repository.GetCustomerByIdAsync(id);

            if (data == null)
                return null;

            return new CustomerDto
            {
                CustomerId = data.CustomerId,
                Name = data.Name,
                Address = data.Address,
                PhoneNumber = data.PhoneNumber,
                Email = data.Email
            };
        }

        public async Task<bool> UpdateCustomerAsync(CustomerDto dto)
        {
            if (dto.CustomerId <= 0)
                throw new Exception("Invalid customer id");

            var customer = new Customer
            {
                CustomerId = dto.CustomerId,
                Name = dto.Name,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email
            };

            return await _repository.UpdateCustomerAsync(customer);
        }
    }
}
