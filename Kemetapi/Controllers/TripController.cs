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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripDto tripDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdTrip = await _tripService.CreateTripAsync(tripDto);
            return CreatedAtAction(nameof(GetTripById), new { id = createdTrip.Id }, createdTrip);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrip(Guid id, [FromBody] UpdateTripDto tripDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedTrip = await _tripService.UpdateTripAsync(id, tripDto);
            if (updatedTrip == null)
            {
                return NotFound();
            }

            return Ok(updatedTrip);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrip(Guid id)
        {
            var result = await _tripService.DeleteTripAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{tripId}/days")]
        public async Task<IActionResult> AddDay(Guid tripId, [FromBody] CreateDayDto dayDto)
        {
             if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdDay = await _tripService.AddDayToTripAsync(tripId, dayDto);
            if (createdDay == null)
            {
                return NotFound("Trip not found");
            }

            return Ok(createdDay);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{tripId}/days/{dayId}")]
        public async Task<IActionResult> RemoveDay(Guid tripId, Guid dayId)
        {
            var result = await _tripService.RemoveDayFromTripAsync(tripId, dayId);
            if (!result)
            {
                return NotFound("Day or Trip not found");
            }
            return NoContent();
        }
    }
}
