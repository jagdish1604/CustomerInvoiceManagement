using CIM.Services.DTOs;
using CIM.Services.Implementations;
using CIM.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;


namespace CIM.API.Controllers
{

    [ApiController]
    [Route("api/customers")]
    [Authorize]
    [EnableRateLimiting("ApiPolicy")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _service;
        private readonly ILogger<CustomerController> _logger;
        private readonly IInvoiceService _invoiceService;

        public CustomerController(ICustomerService service, IInvoiceService invoiceService, ILogger<CustomerController> logger)
        {
            _service = service;
            _invoiceService = invoiceService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
     string? search,
     string? sortBy,
     string? sortDir,
     int page = 1,
     int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Fetching customers");

                
                sortBy = string.IsNullOrWhiteSpace(sortBy) ? "CreatedAt" : sortBy;
                sortDir = string.IsNullOrWhiteSpace(sortDir) ? "desc" : sortDir;
                search = string.IsNullOrWhiteSpace(search) ? null : search;

                var result = await _service.GetCustomersAsync(search, page, pageSize, sortBy, sortDir);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customers");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomerDto dto)
        {
            try
            {
                _logger.LogInformation("Creating a customers");
                var result = await _service.CreateCustomerAsync(dto);
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("UNIQUE"))
                {
                    return BadRequest("Email already exists");
                }
                _logger.LogError(ex, "Error occurred");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteCustomerAsync(id);

                if (!result)
                    return NotFound(new { message = "Customer not found" });

                return Ok(new { message = "Customer deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetCustomerByIdAsync(id);

                if (result == null)
                    return NotFound(new { message = "Customer not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching customer");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CustomerDto dto)
        {
            try
            {
                dto.CustomerId = id;

                var result = await _service.UpdateCustomerAsync(dto);

                if (!result)
                    return NotFound(new { message = "Customer not found" });

                return Ok(new { message = "Customer updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer");
                return BadRequest(ex.Message);
            }
        }
        
    }
}
