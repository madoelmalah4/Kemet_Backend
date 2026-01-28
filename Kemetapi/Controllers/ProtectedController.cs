using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kemet_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProtectedController : ControllerBase
    {
        [HttpGet("user")]
        [Authorize(Roles = "User,Admin")]
        public IActionResult GetUserEndpoint()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                Message = "This is a protected user endpoint",
                UserId = userId,
                Username = username,
                Role = role
            });
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAdminEndpoint()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                Message = "This is a protected admin endpoint",
                UserId = userId,
                Username = username,
                Role = role
            });
        }

        [HttpGet("profile")]
        [Authorize(Roles = "User,Admin")]
        public IActionResult GetUserProfile()
        {
            var claims = new
            {
                Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Username = User.FindFirst(ClaimTypes.Name)?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                Role = User.FindFirst(ClaimTypes.Role)?.Value,
                FirstName = User.FindFirst("FirstName")?.Value,
                LastName = User.FindFirst("LastName")?.Value
            };

            return Ok(claims);
        }
    }
}
