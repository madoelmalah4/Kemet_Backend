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
            try
            {
                var destinations = await _destinationService.GetAllDestinationsAsync();
                return Ok(destinations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving destinations", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var destination = await _destinationService.GetDestinationByIdAsync(id);
                if (destination == null)
                {
                    return NotFound(new { message = $"Destination with ID {id} not found" });
                }
                return Ok(destination);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the destination", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateDestinationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid destination data", errors = ModelState });
            }

            try
            {
                var createdDestination = await _destinationService.CreateDestinationAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = createdDestination.Id }, createdDestination);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = "Validation error", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the destination", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDestinationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid destination data", errors = ModelState });
            }

            try
            {
                var updatedDestination = await _destinationService.UpdateDestinationAsync(id, dto);
                if (updatedDestination == null)
                {
                    return NotFound(new { message = $"Destination with ID {id} not found" });
                }

                return Ok(updatedDestination);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = "Validation error", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the destination", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _destinationService.DeleteDestinationAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Destination with ID {id} not found" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = "Cannot delete destination", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the destination", error = ex.Message });
            }
        }

        [HttpPost("{id}/favorite")]
        [Authorize]
        public async Task<IActionResult> AddFavorite(Guid id)
        {
            try
            {
                var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                    return Unauthorized(new { message = "Invalid user credentials" });

                var result = await _destinationService.AddToFavoritesAsync(userId, id);
                if (!result) 
                    return BadRequest(new { message = "Destination already favorited or not found" });
                
                return Ok(new { message = "Destination added to favorites successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding to favorites", error = ex.Message });
            }
        }

        [HttpDelete("{id}/favorite")]
        [Authorize]
        public async Task<IActionResult> RemoveFavorite(Guid id)
        {
            try
            {
                var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                    return Unauthorized(new { message = "Invalid user credentials" });

                var result = await _destinationService.RemoveFromFavoritesAsync(userId, id);
                if (!result) 
                    return BadRequest(new { message = "Destination not in favorites or not found" });

                return Ok(new { message = "Destination removed from favorites successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing from favorites", error = ex.Message });
            }
        }

        [HttpGet("favorites")]
        [Authorize]
        public async Task<IActionResult> GetFavorites()
        {
            try
            {
                var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                    return Unauthorized(new { message = "Invalid user credentials" });

                var favorites = await _destinationService.GetUserFavoritesAsync(userId);
                return Ok(favorites);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving favorites", error = ex.Message });
            }
        }
    }
}
