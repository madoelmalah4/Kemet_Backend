using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kemet_api.DTOs;
using Kemet_api.Services;
using System.Collections.Concurrent;

namespace Kemet_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TripController : ControllerBase
    {
        private readonly ITripService _tripService;
        // Cache for permission checks within the same request context
        private readonly ConcurrentDictionary<string, bool> _permissionCache = new();

        public TripController(ITripService tripService)
        {
            _tripService = tripService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllTrips()
        {
            try
            {
                var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdString, out var userId)) 
                    return Unauthorized(new { message = "Invalid user credentials" });

                if (User.IsInRole("Admin"))
                {
                    var allTrips = await _tripService.GetAllTripsAsync();
                    return Ok(allTrips);
                }

                var userTrips = await _tripService.GetUserTripsAsync(userId);
                return Ok(userTrips);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving trips", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTripById(Guid id)
        {
            try
            {
                var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdString, out var userId)) 
                    return Unauthorized(new { message = "Invalid user credentials" });

                TripDto? trip;
                if (User.IsInRole("Admin"))
                {
                    trip = await _tripService.GetTripByIdAsync(id);
                }
                else
                {
                    trip = await _tripService.GetUserTripByIdAsync(id, userId);
                }

                if (trip == null) 
                    return NotFound(new { message = $"Trip with ID {id} not found or you don't have permission to access it" });
                
                return Ok(trip);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the trip", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripDto tripDto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(new { message = "Invalid trip data", errors = ModelState });

            try
            {
                Guid? userId = null;
                var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdString, out var uid)) userId = uid;

                // If Admin, create as System Trip (userId = null). 
                // If User, create as User Trip.
                if (User.IsInRole("Admin"))
                {
                    userId = null;
                }
                else if (userId == null)
                {
                    return Unauthorized(new { message = "User authentication required" });
                }

                var createdTrip = await _tripService.CreateTripAsync(tripDto, userId);
                return CreatedAtAction(nameof(GetTripById), new { id = createdTrip.Id }, createdTrip);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = "Validation error", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the trip", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrip(Guid id, [FromBody] UpdateTripDto tripDto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(new { message = "Invalid trip data", errors = ModelState });

            try
            {
                if (!await CanEditTrip(id)) 
                    return Forbid();

                var updatedTrip = await _tripService.UpdateTripAsync(id, tripDto);
                if (updatedTrip == null) 
                    return NotFound(new { message = $"Trip with ID {id} not found" });

                return Ok(updatedTrip);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = "Validation error", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the trip", error = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrip(Guid id)
        {
            try
            {
                if (!await CanEditTrip(id)) 
                    return Forbid();

                var result = await _tripService.DeleteTripAsync(id);
                if (!result) 
                    return NotFound(new { message = $"Trip with ID {id} not found" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the trip", error = ex.Message });
            }
        }

        // Days
        [Authorize]
        [HttpPost("{tripId}/days")]
        public async Task<IActionResult> AddDay(Guid tripId, [FromBody] CreateDayDto dayDto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(new { message = "Invalid day data", errors = ModelState });

            try
            {
                if (!await CanEditTrip(tripId)) 
                    return Forbid();

                var createdDay = await _tripService.AddDayToTripAsync(tripId, dayDto);
                if (createdDay == null) 
                    return NotFound(new { message = $"Trip with ID {tripId} not found" });

                return Ok(createdDay);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = "Validation error", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the day", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("{tripId}/days/{dayId}")]
        public async Task<IActionResult> UpdateDay(Guid tripId, Guid dayId, [FromBody] UpdateDayDto dayDto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(new { message = "Invalid day data", errors = ModelState });

            try
            {
                if (!await CanEditTrip(tripId)) 
                    return Forbid();

                var updatedDay = await _tripService.UpdateDayAsync(tripId, dayId, dayDto);
                if (updatedDay == null) 
                    return NotFound(new { message = $"Day with ID {dayId} not found in trip {tripId}" });

                return Ok(updatedDay);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = "Validation error", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the day", error = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("{tripId}/days/{dayId}")]
        public async Task<IActionResult> RemoveDay(Guid tripId, Guid dayId)
        {
            try
            {
                if (!await CanEditTrip(tripId)) 
                    return Forbid();

                var result = await _tripService.RemoveDayFromTripAsync(tripId, dayId);
                if (!result) 
                    return NotFound(new { message = $"Day with ID {dayId} not found in trip {tripId}" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing the day", error = ex.Message });
            }
        }

        // Activities
        [Authorize]
        [HttpPost("{tripId}/days/{dayId}/activities")]
        public async Task<IActionResult> AddActivity(Guid tripId, Guid dayId, [FromBody] CreateDayActivityDto activityDto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(new { message = "Invalid activity data", errors = ModelState });

            try
            {
                if (!await CanEditTrip(tripId)) 
                    return Forbid();
                
                var createdActivity = await _tripService.AddActivityToDayAsync(dayId, activityDto);
                if (createdActivity == null) 
                    return NotFound(new { message = $"Day with ID {dayId} not found" });

                return Ok(createdActivity);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = "Validation error", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding the activity", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("{tripId}/days/{dayId}/activities/{activityId}")]
        public async Task<IActionResult> UpdateActivity(Guid tripId, Guid dayId, Guid activityId, [FromBody] UpdateDayActivityDto activityDto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(new { message = "Invalid activity data", errors = ModelState });

            try
            {
                if (!await CanEditTrip(tripId)) 
                    return Forbid();

                var updatedActivity = await _tripService.UpdateActivityAsync(dayId, activityId, activityDto);
                if (updatedActivity == null) 
                    return NotFound(new { message = $"Activity with ID {activityId} not found in day {dayId}" });

                return Ok(updatedActivity);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = "Validation error", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the activity", error = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("{tripId}/days/{dayId}/activities/{activityId}")]
        public async Task<IActionResult> RemoveActivity(Guid tripId, Guid dayId, Guid activityId)
        {
            try
            {
                if (!await CanEditTrip(tripId)) 
                    return Forbid();

                var result = await _tripService.RemoveActivityAsync(dayId, activityId);
                if (!result) 
                    return NotFound(new { message = $"Activity with ID {activityId} not found in day {dayId}" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing the activity", error = ex.Message });
            }
        }

        private async Task<bool> CanEditTrip(Guid tripId)
        {
            // Use cache key to avoid redundant permission checks in the same request
            var cacheKey = $"{tripId}_{User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value}";
            
            if (_permissionCache.TryGetValue(cacheKey, out var cachedResult))
            {
                return cachedResult;
            }

            if (User.IsInRole("Admin"))
            {
                _permissionCache[cacheKey] = true;
                return true;
            }

            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out var userId))
            {
                _permissionCache[cacheKey] = false;
                return false;
            }

            // Efficiently check ownership using the service method that filters by UserId
            var trip = await _tripService.GetUserTripByIdAsync(tripId, userId);
            var hasPermission = trip != null;
            
            _permissionCache[cacheKey] = hasPermission;
            return hasPermission;
        }
    }
}
