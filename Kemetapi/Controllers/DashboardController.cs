using Microsoft.AspNetCore.Mvc;
using Kemet_api.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims;

namespace Kemet_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        // [Authorize(Roles = "Admin")] // Uncomment if only admins should see this
        public async Task<IActionResult> GetDashboardData()
        {
            var data = await _dashboardService.GetDashboardDataAsync();
            return Ok(data);
        }

        [HttpPost("track")]
        public async Task<IActionResult> TrackEvent([FromBody] TrackEventRequest request)
        {
            Guid? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out var parsedId))
                {
                    userId = parsedId;
                }
            }

            await _dashboardService.TrackEventAsync(request.EventType, request.Category, userId);
            return Ok();
        }
    }

    public class TrackEventRequest
    {
        public string EventType { get; set; } = string.Empty;
        public string? Category { get; set; }
    }
}
