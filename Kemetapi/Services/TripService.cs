using Kemet_api.DTOs;
using Kemet_api.Models;
using Kemet_api.Repositories;

namespace Kemet_api.Services
{
    public class TripService : ITripService
    {
        private readonly ITripRepository _tripRepository;
        private readonly IRepository<Day> _dayRepository;
        private readonly IRepository<DayActivity> _dayActivityRepository;
        private readonly IDestinationRepository _destinationRepository;

        public TripService(ITripRepository tripRepository, 
                           IRepository<Day> dayRepository,
                           IRepository<DayActivity> dayActivityRepository,
                           IDestinationRepository destinationRepository)
        {
            _tripRepository = tripRepository;
            _dayRepository = dayRepository;
            _dayActivityRepository = dayActivityRepository;
            _destinationRepository = destinationRepository;
        }

        public async Task<IEnumerable<TripDto>> GetAllTripsAsync()
        {
            var trips = await _tripRepository.GetAllWithDaysAsync();
            return trips.Select(MapToDto);
        }
        
        public async Task<IEnumerable<TripDto>> GetUserTripsAsync(Guid userId)
        {
            var trips = await _tripRepository.GetTripsByUserIdAsync(userId);
            return trips.Select(MapToDto);
        }

        public async Task<TripDto?> GetTripByIdAsync(Guid id)
        {
            var trip = await _tripRepository.GetTripWithDaysAsync(id);
            if (trip == null) return null;
            return MapToDto(trip);
        }

        public async Task<TripDto?> GetUserTripByIdAsync(Guid tripId, Guid userId)
        {
            var trip = await _tripRepository.GetUserTripWithDaysAsync(tripId, userId);
            if (trip == null) return null;
            return MapToDto(trip);
        }

        public async Task<TripDto> CreateTripAsync(CreateTripDto tripDto, Guid? userId = null)
        {
            var trip = new Trip
            {
                UserId = userId,
                Title = tripDto.Title,
                TravelCompanions = tripDto.TravelCompanions,
                TravelStyle = tripDto.TravelStyle,
                ExperienceTypes = tripDto.ExperienceTypes,
                Interests = tripDto.Interests,
                StartDate = tripDto.StartDate,
                EndDate = tripDto.EndDate,
                DurationDays = tripDto.DurationDays,
                Price = tripDto.Price,
                Description = tripDto.Description,
                ImageUrl = tripDto.ImageUrl,
                CreatedAt = DateTime.UtcNow,
                Days = tripDto.Days?.Select(d => new Day
                {
                    DayNumber = d.DayNumber,
                    Date = d.Date,
                    Title = d.Title,
                    Description = d.Description,
                    City = d.City,
                    DayActivities = d.Activities?.Select(a => new DayActivity
                    {
                        DestinationId = a.DestinationId,
                        ActivityType = a.ActivityType,
                        StartTime = a.StartTime,
                        DurationHours = a.DurationHours,
                        Description = a.Description
                    }).ToList() ?? new List<DayActivity>()
                }).ToList() ?? new List<Day>()
            };

            await _tripRepository.AddAsync(trip);
            
            // Reload with all includes to get Destination names etc.
            var result = await _tripRepository.GetTripWithDaysAsync(trip.Id);
            return MapToDto(result!);
        }

        public async Task<TripDto?> UpdateTripAsync(Guid id, UpdateTripDto tripDto)
        {
            var trip = await _tripRepository.GetTripWithDaysAsync(id);
            if (trip == null) return null;

            trip.Title = tripDto.Title;
            trip.TravelCompanions = tripDto.TravelCompanions;
            trip.TravelStyle = tripDto.TravelStyle;
            trip.ExperienceTypes = tripDto.ExperienceTypes;
            trip.Interests = tripDto.Interests;
            trip.StartDate = tripDto.StartDate;
            trip.EndDate = tripDto.EndDate;
            trip.DurationDays = tripDto.DurationDays;
            trip.Price = tripDto.Price;
            trip.Description = tripDto.Description;
            trip.ImageUrl = tripDto.ImageUrl;

            await _tripRepository.UpdateAsync(trip);
            return MapToDto(trip);
        }

        public async Task<bool> DeleteTripAsync(Guid id)
        {
            var trip = await _tripRepository.GetByIdAsync(id);
            if (trip == null) return false;

            await _tripRepository.DeleteAsync(trip);
            return true;
        }

        public async Task<DayDto?> AddDayToTripAsync(Guid tripId, CreateDayDto dayDto)
        {
            var trip = await _tripRepository.GetByIdAsync(tripId);
            if (trip == null) return null;

            var day = new Day
            {
                TripId = tripId,
                DayNumber = dayDto.DayNumber,
                Date = dayDto.Date,
                Title = dayDto.Title,
                Description = dayDto.Description,
                City = dayDto.City,
                DayActivities = dayDto.Activities?.Select(a => new DayActivity
                {
                    DestinationId = a.DestinationId,
                    ActivityType = a.ActivityType,
                    StartTime = a.StartTime,
                    DurationHours = a.DurationHours,
                    Description = a.Description
                }).ToList() ?? new List<DayActivity>()
            };

            await _dayRepository.AddAsync(day);
            return MapToDayDto(day);
        }

        public async Task<DayDto?> UpdateDayAsync(Guid tripId, Guid dayId, UpdateDayDto dayDto)
        {
            var day = await _dayRepository.GetByIdAsync(dayId);
            if (day == null || day.TripId != tripId) return null;

            day.DayNumber = dayDto.DayNumber;
            day.Date = dayDto.Date;
            day.Title = dayDto.Title;
            day.Description = dayDto.Description;
            day.City = dayDto.City;

            await _dayRepository.UpdateAsync(day);
            return MapToDayDto(day);
        }

        public async Task<bool> RemoveDayFromTripAsync(Guid tripId, Guid dayId)
        {
            var day = await _dayRepository.GetByIdAsync(dayId);
            if (day == null || day.TripId != tripId) return false;

            await _dayRepository.DeleteAsync(day);
            return true;
        }

        public async Task<DayActivityDto?> AddActivityToDayAsync(Guid dayId, CreateDayActivityDto activityDto)
        {
            var day = await _dayRepository.GetByIdAsync(dayId);
            if (day == null) return null;

            var destination = await _destinationRepository.GetByIdAsync(activityDto.DestinationId);
            if (destination == null) return null;

            var activity = new DayActivity
            {
                DayId = dayId,
                DestinationId = activityDto.DestinationId,
                ActivityType = activityDto.ActivityType,
                StartTime = activityDto.StartTime,
                DurationHours = activityDto.DurationHours,
                Description = activityDto.Description
            };

            await _dayActivityRepository.AddAsync(activity);
            
            // Manually populate Destination for DTO
            activity.Destination = destination; 
            return MapToDayActivityDto(activity);
        }

        public async Task<DayActivityDto?> UpdateActivityAsync(Guid dayId, Guid activityId, UpdateDayActivityDto activityDto)
        {
            var activity = await _dayActivityRepository.GetByIdAsync(activityId);
            if (activity == null || activity.DayId != dayId) return null;

            activity.ActivityType = activityDto.ActivityType;
            activity.StartTime = activityDto.StartTime;
            activity.DurationHours = activityDto.DurationHours;
            activity.Description = activityDto.Description;
            
            if (activity.DestinationId != activityDto.DestinationId)
            {
                 var destination = await _destinationRepository.GetByIdAsync(activityDto.DestinationId);
                 if (destination == null) return null;
                 activity.DestinationId = activityDto.DestinationId;
                 activity.Destination = destination;
            }
            else
            {
                 // Ensure destination is loaded if possible, otherwise fetch
                 if (activity.Destination == null) 
                 {
                     activity.Destination = await _destinationRepository.GetByIdAsync(activity.DestinationId);
                 }
            }

            await _dayActivityRepository.UpdateAsync(activity);
            return MapToDayActivityDto(activity);
        }

        public async Task<bool> RemoveActivityAsync(Guid dayId, Guid activityId)
        {
            var activity = await _dayActivityRepository.GetByIdAsync(activityId);
            if (activity == null || activity.DayId != dayId) return false;

            await _dayActivityRepository.DeleteAsync(activity);
            return true;
        }

        private TripDto MapToDto(Trip trip)
        {
            return new TripDto
            {
                Id = trip.Id,
                UserId = trip.UserId,
                Title = trip.Title,
                TravelCompanions = trip.TravelCompanions,
                TravelStyle = trip.TravelStyle,
                ExperienceTypes = trip.ExperienceTypes,
                Interests = trip.Interests,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                DurationDays = trip.DurationDays,
                Price = trip.Price,
                Description = trip.Description,
                ImageUrl = trip.ImageUrl,
                CreatedAt = trip.CreatedAt,
                Days = trip.Days?.Select(MapToDayDto).ToList() ?? new List<DayDto>()
            };
        }

        private DayDto MapToDayDto(Day day)
        {
            return new DayDto
            {
                Id = day.Id,
                TripId = day.TripId,
                DayNumber = day.DayNumber,
                Date = day.Date,
                Title = day.Title,
                Description = day.Description,
                City = day.City,
                Activities = day.DayActivities?.Select(MapToDayActivityDto).ToList() ?? new List<DayActivityDto>()
            };
        }

        private DayActivityDto MapToDayActivityDto(DayActivity activity)
        {
            return new DayActivityDto
            {
                Id = activity.Id,
                DayId = activity.DayId,
                DestinationId = activity.DestinationId,
                DestinationName = activity.Destination?.Name ?? "Unknown",
                DestinationImageUrl = activity.Destination?.ImageUrl,
                ActivityType = activity.ActivityType,
                StartTime = activity.StartTime,
                DurationHours = activity.DurationHours,
                Description = activity.Description
            };
        }
    }
}
