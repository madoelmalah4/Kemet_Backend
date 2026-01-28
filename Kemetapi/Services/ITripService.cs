using Kemet_api.DTOs;
using Kemet_api.Models;

namespace Kemet_api.Services
{
    public interface ITripService
    {
        Task<IEnumerable<TripDto>> GetAllTripsAsync();
        Task<TripDto?> GetTripByIdAsync(Guid id);
        Task<TripDto> CreateTripAsync(CreateTripDto tripDto);
        Task<TripDto?> UpdateTripAsync(Guid id, UpdateTripDto tripDto);
        Task<bool> DeleteTripAsync(Guid id);
        Task<DayDto?> AddDayToTripAsync(Guid tripId, CreateDayDto dayDto);
        Task<bool> RemoveDayFromTripAsync(Guid tripId, Guid dayId);
    }
}
