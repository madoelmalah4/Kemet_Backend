using Kemet_api.DTOs;
using Kemet_api.Models;
using Kemet_api.Repositories;
using Microsoft.EntityFrameworkCore;
using Kemet_api.Data;

namespace Kemet_api.Services
{
    public class TripService : ITripService
    {
        private readonly ITripRepository _tripRepository;
        private readonly IRepository<Day> _dayRepository;
        private readonly IRepository<DayActivity> _dayActivityRepository;
        private readonly IDestinationRepository _destinationRepository;
        private readonly ApplicationDbContext _context;

        public TripService(ITripRepository tripRepository, 
                           IRepository<Day> dayRepository,
                           IRepository<DayActivity> dayActivityRepository,
                           IDestinationRepository destinationRepository,
                           ApplicationDbContext context)
        {
            _tripRepository = tripRepository;
            _dayRepository = dayRepository;
            _dayActivityRepository = dayActivityRepository;
            _destinationRepository = destinationRepository;
            _context = context;
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
            // Validate date range
            if (tripDto.EndDate < tripDto.StartDate)
            {
                throw new ArgumentException("End date must be after start date");
            }

            // Validate duration matches date range
            var calculatedDuration = (tripDto.EndDate - tripDto.StartDate).Days + 1;
            if (tripDto.DurationDays != calculatedDuration)
            {
                throw new ArgumentException($"Duration days ({tripDto.DurationDays}) doesn't match date range ({calculatedDuration} days)");
            }

            // Validate destination IDs if activities are provided
            if (tripDto.Days?.Any() == true)
            {
                var allDestinationIds = tripDto.Days
                    .Where(d => d.Activities != null)
                    .SelectMany(d => d.Activities!)
                    .Select(a => a.DestinationId)
                    .Distinct()
                    .ToList();

                if (allDestinationIds.Any())
                {
                    var existingDestinations = await _destinationRepository.GetAllAsync();
                    var existingIds = existingDestinations.Select(d => d.Id).ToHashSet();
                    var invalidIds = allDestinationIds.Where(id => !existingIds.Contains(id)).ToList();
                    
                    if (invalidIds.Any())
                    {
                        throw new ArgumentException($"Invalid destination IDs: {string.Join(", ", invalidIds)}");
                    }
                }

                // Validate day numbers are sequential and start from 1
                var dayNumbers = tripDto.Days.Select(d => d.DayNumber).OrderBy(n => n).ToList();
                if (dayNumbers.First() != 1 || dayNumbers.Count != dayNumbers.Last())
                {
                    throw new ArgumentException("Day numbers must be sequential starting from 1");
                }
            }

            // Use transaction for atomic operation
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var trip = new Trip
                {
                    UserId = userId,
                    Title = tripDto.Title,
                    TravelCompanions = tripDto.TravelCompanions,
                    TravelStyle = tripDto.TravelStyle,
                    ExperienceTypes = tripDto.ExperienceTypes ?? new List<string>(),
                    Interests = tripDto.Interests ?? new List<string>(),
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
                await transaction.CommitAsync();
                
                // Reload with all includes to get Destination names etc.
                var result = await _tripRepository.GetTripWithDaysAsync(trip.Id);
                return MapToDto(result!);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<TripDto?> UpdateTripAsync(Guid id, UpdateTripDto tripDto)
        {
            // Validate date range
            if (tripDto.EndDate < tripDto.StartDate)
            {
                throw new ArgumentException("End date must be after start date");
            }

            var trip = await _tripRepository.GetTripWithDaysAsync(id);
            if (trip == null) return null;

            trip.Title = tripDto.Title;
            trip.TravelCompanions = tripDto.TravelCompanions;
            trip.TravelStyle = tripDto.TravelStyle;
            trip.ExperienceTypes = tripDto.ExperienceTypes ?? new List<string>();
            trip.Interests = tripDto.Interests ?? new List<string>();
            trip.StartDate = tripDto.StartDate;
            trip.EndDate = tripDto.EndDate;
            trip.DurationDays = tripDto.DurationDays;
            trip.Price = tripDto.Price;
            trip.Description = tripDto.Description;
            trip.ImageUrl = tripDto.ImageUrl;

            await _tripRepository.UpdateAsync(trip);
            
            // Reload to ensure all navigation properties are fresh
            var updatedTrip = await _tripRepository.GetTripWithDaysAsync(id);
            return MapToDto(updatedTrip!);
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
            var trip = await _tripRepository.GetTripWithDaysAsync(tripId);
            if (trip == null) return null;

            // Validate day number doesn't already exist
            if (trip.Days?.Any(d => d.DayNumber == dayDto.DayNumber) == true)
            {
                throw new ArgumentException($"Day number {dayDto.DayNumber} already exists for this trip");
            }

            // Validate destination IDs if activities are provided
            if (dayDto.Activities?.Any() == true)
            {
                var destinationIds = dayDto.Activities.Select(a => a.DestinationId).Distinct().ToList();
                var existingDestinations = await _destinationRepository.GetAllAsync();
                var existingIds = existingDestinations.Select(d => d.Id).ToHashSet();
                var invalidIds = destinationIds.Where(id => !existingIds.Contains(id)).ToList();
                
                if (invalidIds.Any())
                {
                    throw new ArgumentException($"Invalid destination IDs: {string.Join(", ", invalidIds)}");
                }
            }

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
            
            // Reload with includes to get destination data
            var reloadedDay = await _context.Days
                .Include(d => d.DayActivities)
                    .ThenInclude(da => da.Destination)
                .FirstOrDefaultAsync(d => d.Id == day.Id);
                
            return MapToDayDto(reloadedDay!);
        }

        public async Task<DayDto?> UpdateDayAsync(Guid tripId, Guid dayId, UpdateDayDto dayDto)
        {
            var day = await _context.Days
                .Include(d => d.DayActivities)
                    .ThenInclude(da => da.Destination)
                .FirstOrDefaultAsync(d => d.Id == dayId);
                
            if (day == null || day.TripId != tripId) return null;

            // Check if day number is being changed and if it conflicts
            if (day.DayNumber != dayDto.DayNumber)
            {
                var conflictExists = await _context.Days
                    .AnyAsync(d => d.TripId == tripId && d.DayNumber == dayDto.DayNumber && d.Id != dayId);
                    
                if (conflictExists)
                {
                    throw new ArgumentException($"Day number {dayDto.DayNumber} already exists for this trip");
                }
            }

            day.DayNumber = dayDto.DayNumber;
            day.Date = dayDto.Date;
            day.Title = dayDto.Title;
            day.Description = dayDto.Description;
            day.City = dayDto.City;

            await _dayRepository.UpdateAsync(day);
            
            // Reload to ensure fresh data
            var updatedDay = await _context.Days
                .Include(d => d.DayActivities)
                    .ThenInclude(da => da.Destination)
                .FirstOrDefaultAsync(d => d.Id == dayId);
                
            return MapToDayDto(updatedDay!);
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
            if (destination == null) 
            {
                throw new ArgumentException($"Destination with ID {activityDto.DestinationId} not found");
            }

            // Validate duration is positive
            if (activityDto.DurationHours <= 0)
            {
                throw new ArgumentException("Duration must be greater than 0");
            }

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
            var activity = await _context.DayActivities
                .Include(a => a.Destination)
                .FirstOrDefaultAsync(a => a.Id == activityId);
                
            if (activity == null || activity.DayId != dayId) return null;

            // Validate duration is positive
            if (activityDto.DurationHours <= 0)
            {
                throw new ArgumentException("Duration must be greater than 0");
            }

            activity.ActivityType = activityDto.ActivityType;
            activity.StartTime = activityDto.StartTime;
            activity.DurationHours = activityDto.DurationHours;
            activity.Description = activityDto.Description;
            
            if (activity.DestinationId != activityDto.DestinationId)
            {
                 var destination = await _destinationRepository.GetByIdAsync(activityDto.DestinationId);
                 if (destination == null) 
                 {
                     throw new ArgumentException($"Destination with ID {activityDto.DestinationId} not found");
                 }
                 activity.DestinationId = activityDto.DestinationId;
                 activity.Destination = destination;
            }

            await _dayActivityRepository.UpdateAsync(activity);
            
            // Reload to ensure fresh data
            var updatedActivity = await _context.DayActivities
                .Include(a => a.Destination)
                .FirstOrDefaultAsync(a => a.Id == activityId);
                
            return MapToDayActivityDto(updatedActivity!);
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
                ExperienceTypes = trip.ExperienceTypes ?? new List<string>(),
                Interests = trip.Interests ?? new List<string>(),
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                DurationDays = trip.DurationDays,
                Price = trip.Price,
                Description = trip.Description,
                ImageUrl = trip.ImageUrl,
                CreatedAt = trip.CreatedAt,
                Days = trip.Days?.OrderBy(d => d.DayNumber).Select(MapToDayDto).ToList() ?? new List<DayDto>()
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
                Activities = day.DayActivities?.OrderBy(a => a.StartTime).Select(MapToDayActivityDto).ToList() ?? new List<DayActivityDto>()
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
