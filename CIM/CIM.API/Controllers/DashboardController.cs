using CIM.Repository.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace CIM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardRepository _repo;

        public DashboardController(DashboardRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await _repo.GetDashboardAsync();

            return Ok(new
            {
                totalCustomers = result.Item1,
                totalInvoices = result.Item2,
                totalAmount = result.Item3
            });
        }
    }


}
