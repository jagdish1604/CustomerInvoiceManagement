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
    [Route("api/invoices")]
    [Authorize]
    [EnableRateLimiting("ApiPolicy")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _service;
        private readonly ILogger<CustomerController> _logger;

        public InvoiceController(IInvoiceService service, ILogger<CustomerController> logger)
        {
            _service = service;
            _logger = logger;
        }

       
        [HttpPost]
        public async Task<IActionResult> Create(InvoiceDto dto)
        {
            try
            {
                var result = await _service.CreateInvoiceAsync(dto);
                return StatusCode(201, new { message = "Invoice created successfully", id = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                return BadRequest(ex.Message);
            }
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
                search = string.IsNullOrWhiteSpace(search) ? null : search;
                sortBy = string.IsNullOrWhiteSpace(sortBy) ? "CreatedAt" : sortBy;
                sortDir = string.IsNullOrWhiteSpace(sortDir) ? "desc" : sortDir;

                var result = await _service.GetInvoicesAsync(search, page, pageSize, sortBy, sortDir);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invoices");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetInvoiceByIdAsync(id);

                if (result == null)
                    return NotFound(new { message = "Invoice not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invoice");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, InvoiceDto dto)
        {
            try
            {
                var result = await _service.UpdateInvoiceAsync(id, dto);

                if (!result)
                    return NotFound(new { message = "Invoice not found" });

                return Ok(new { message = "Invoice updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice");
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteInvoiceAsync(id);

                if (!result)
                    return NotFound(new { message = "Invoice not found" });

                return Ok(new { message = "Invoice deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting invoice");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("dropdown")]
        public async Task<IActionResult> GetDropdown()
        {
            try
            {
                var data = await _service.GetCustomerDropdownAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dropdown");
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
