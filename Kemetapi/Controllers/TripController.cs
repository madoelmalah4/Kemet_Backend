using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kemet_api.DTOs;
using Kemet_api.Services;

namespace Kemet_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TripController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripController(ITripService tripService)
        {
            _tripService = tripService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTrips()
        {
            // TODO: Filter based on user? Existing requirement implies public listing.
            var trips = await _tripService.GetAllTripsAsync();
            return Ok(trips);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTripById(Guid id)
        {
            var trip = await _tripService.GetTripByIdAsync(id);
            if (trip == null)
            {
                return NotFound();
            }
            return Ok(trip);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripDto tripDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

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
                return Unauthorized();
            }

            var createdTrip = await _tripService.CreateTripAsync(tripDto, userId);
            return CreatedAtAction(nameof(GetTripById), new { id = createdTrip.Id }, createdTrip);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrip(Guid id, [FromBody] UpdateTripDto tripDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!await CanEditTrip(id)) return Forbid();

            var updatedTrip = await _tripService.UpdateTripAsync(id, tripDto);
            if (updatedTrip == null) return NotFound();

            return Ok(updatedTrip);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrip(Guid id)
        {
            if (!await CanEditTrip(id)) return Forbid();

            var result = await _tripService.DeleteTripAsync(id);
            if (!result) return NotFound();

            return NoContent();
        }

        // Days
        [Authorize]
        [HttpPost("{tripId}/days")]
        public async Task<IActionResult> AddDay(Guid tripId, [FromBody] CreateDayDto dayDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!await CanEditTrip(tripId)) return Forbid();

            var createdDay = await _tripService.AddDayToTripAsync(tripId, dayDto);
            if (createdDay == null) return NotFound("Trip not found");

            return Ok(createdDay);
        }

        [Authorize]
        [HttpPut("{tripId}/days/{dayId}")]
        public async Task<IActionResult> UpdateDay(Guid tripId, Guid dayId, [FromBody] UpdateDayDto dayDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!await CanEditTrip(tripId)) return Forbid();

            var updatedDay = await _tripService.UpdateDayAsync(tripId, dayId, dayDto);
            if (updatedDay == null) return NotFound("Day or Trip not found");

            return Ok(updatedDay);
        }

        [Authorize]
        [HttpDelete("{tripId}/days/{dayId}")]
        public async Task<IActionResult> RemoveDay(Guid tripId, Guid dayId)
        {
            if (!await CanEditTrip(tripId)) return Forbid();

            var result = await _tripService.RemoveDayFromTripAsync(tripId, dayId);
            if (!result) return NotFound("Day or Trip not found");

            return NoContent();
        }

        // Activities
        [Authorize]
        [HttpPost("{tripId}/days/{dayId}/activities")]
        public async Task<IActionResult> AddActivity(Guid tripId, Guid dayId, [FromBody] CreateDayActivityDto activityDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!await CanEditTrip(tripId)) return Forbid();
            
            // Verify day belongs to trip implicitly handled by permissions on Trip, 
            // but Service should verify day exists. Service verifies day exists.
            // But we should verify Trip ownership first.
            
            var createdActivity = await _tripService.AddActivityToDayAsync(dayId, activityDto);
            if (createdActivity == null) return NotFound("Day or Destination not found");

            return Ok(createdActivity);
        }

        [Authorize]
        [HttpPut("{tripId}/days/{dayId}/activities/{activityId}")]
        public async Task<IActionResult> UpdateActivity(Guid tripId, Guid dayId, Guid activityId, [FromBody] UpdateDayActivityDto activityDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!await CanEditTrip(tripId)) return Forbid();

            var updatedActivity = await _tripService.UpdateActivityAsync(dayId, activityId, activityDto);
            if (updatedActivity == null) return NotFound("Activity or Day not found");

            return Ok(updatedActivity);
        }

        [Authorize]
        [HttpDelete("{tripId}/days/{dayId}/activities/{activityId}")]
        public async Task<IActionResult> RemoveActivity(Guid tripId, Guid dayId, Guid activityId)
        {
            if (!await CanEditTrip(tripId)) return Forbid();

            var result = await _tripService.RemoveActivityAsync(dayId, activityId);
            if (!result) return NotFound("Activity or Day not found");

            return NoContent();
        }

        private async Task<bool> CanEditTrip(Guid tripId)
        {
            var trip = await _tripService.GetTripByIdAsync(tripId);
            if (trip == null) return false;

            if (User.IsInRole("Admin")) return true;

            var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdString, out var userId))
            {
                return trip.UserId == userId;
            }
            return false;
        }
    }
}
