using Kemet_api.DTOs.Destination;
using Kemet_api.Models;
using Kemet_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kemet_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DestinationsController : ControllerBase
    {
        private readonly IDestinationService _destinationService;

        public DestinationsController(IDestinationService destinationService)
        {
            _destinationService = destinationService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var destinations = await _destinationService.GetAllDestinationsAsync();
            return Ok(destinations);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var destination = await _destinationService.GetDestinationByIdAsync(id);
            if (destination == null)
            {
                return NotFound("Destination not found.");
            }
            return Ok(destination);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateDestinationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdDestination = await _destinationService.CreateDestinationAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdDestination.Id }, createdDestination);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDestinationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedDestination = await _destinationService.UpdateDestinationAsync(id, dto);
            if (updatedDestination == null)
            {
                return NotFound("Destination not found.");
            }

            return Ok(updatedDestination);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _destinationService.DeleteDestinationAsync(id);
            if (!result)
            {
                return NotFound("Destination not found.");
            }

            return NoContent();
        }
        [HttpPost("{id}/favorite")]
        [Authorize]
        public async Task<IActionResult> AddFavorite(Guid id)
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            var result = await _destinationService.AddToFavoritesAsync(userId, id);
            if (!result) return BadRequest("Already favorited or destination not found.");
            
            return Ok(new { message = "Added to favorites." });
        }

        [HttpDelete("{id}/favorite")]
        [Authorize]
        public async Task<IActionResult> RemoveFavorite(Guid id)
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            var result = await _destinationService.RemoveFromFavoritesAsync(userId, id);
            if (!result) return BadRequest("Not favorited or destination not found.");

            return Ok(new { message = "Removed from favorites." });
        }

        [HttpGet("favorites")]
        [Authorize]
        public async Task<IActionResult> GetFavorites()
        {
            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            var favorites = await _destinationService.GetUserFavoritesAsync(userId);
            return Ok(favorites);
        }
    }
}
