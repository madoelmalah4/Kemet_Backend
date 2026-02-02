using Microsoft.AspNetCore.Mvc;
using Kemet_api.Models;
using Kemet_api.Services;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace Kemet_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AdminController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _authService.GetAllUsersAsync();
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role
            });
            return Ok(userDtos);
        }

        [HttpPatch("users/{userId:guid}/role")]
        public async Task<IActionResult> UpdateUserRole(Guid userId, [FromBody] UpdateRoleRequest request)
        {
            var (success, message) = await _authService.UpdateUserRoleAsync(userId, request.NewRole);
            if (!success) return BadRequest(new { message });
            return Ok(new { message });
        }
    }

    public class UpdateRoleRequest
    {
        public UserRole NewRole { get; set; }
    }
}
