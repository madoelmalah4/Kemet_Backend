using Kemet_api.DTOs;
using Kemet_api.Models;

namespace Kemet_api.Services
{
    public interface ITripService
    {
        Task<IEnumerable<TripDto>> GetAllTripsAsync();
        Task<IEnumerable<TripDto>> GetUserTripsAsync(Guid userId);
        Task<TripDto?> GetTripByIdAsync(Guid id);
        Task<TripDto?> GetUserTripByIdAsync(Guid tripId, Guid userId);
        Task<TripDto> CreateTripAsync(CreateTripDto tripDto, Guid? userId = null);
        Task<TripDto?> UpdateTripAsync(Guid id, UpdateTripDto tripDto);
        Task<bool> DeleteTripAsync(Guid id);
        Task<DayDto?> AddDayToTripAsync(Guid tripId, CreateDayDto dayDto);
        Task<DayDto?> UpdateDayAsync(Guid tripId, Guid dayId, UpdateDayDto dayDto);
        Task<bool> RemoveDayFromTripAsync(Guid tripId, Guid dayId);
        
        Task<DayActivityDto?> AddActivityToDayAsync(Guid dayId, CreateDayActivityDto activityDto);
        Task<DayActivityDto?> UpdateActivityAsync(Guid dayId, Guid activityId, UpdateDayActivityDto activityDto);
        Task<bool> RemoveActivityAsync(Guid dayId, Guid activityId);
    }
}
