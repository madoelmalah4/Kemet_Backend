using Kemet_api.DTOs;
using Kemet_api.Models;
using Kemet_api.Repositories;

namespace Kemet_api.Services
{
    public class TripService : ITripService
    {
        private readonly ITripRepository _tripRepository;
        private readonly IRepository<Day> _dayRepository;

        public TripService(ITripRepository tripRepository, IRepository<Day> dayRepository)
        {
            _tripRepository = tripRepository;
            _dayRepository = dayRepository;
        }

        public async Task<IEnumerable<TripDto>> GetAllTripsAsync()
        {
            var trips = await _tripRepository.GetAllWithDaysAsync();
            return trips.Select(MapToDto);
        }

        public async Task<TripDto?> GetTripByIdAsync(Guid id)
        {
            var trip = await _tripRepository.GetTripWithDaysAsync(id);
            if (trip == null) return null;
            return MapToDto(trip);
        }

        public async Task<TripDto> CreateTripAsync(CreateTripDto tripDto)
        {
            var trip = new Trip
            {
                Title = tripDto.Title,
                TripType = tripDto.TripType,
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
                    Description = d.Description
                }).ToList() ?? new List<Day>()
            };

            await _tripRepository.AddAsync(trip);
            return MapToDto(trip);
        }

        public async Task<TripDto?> UpdateTripAsync(Guid id, UpdateTripDto tripDto)
        {
            var trip = await _tripRepository.GetTripWithDaysAsync(id);
            if (trip == null) return null;

            trip.Title = tripDto.Title;
            trip.TripType = tripDto.TripType;
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
                Description = dayDto.Description
            };

            await _dayRepository.AddAsync(day);
            return MapToDayDto(day);
        }

        public async Task<bool> RemoveDayFromTripAsync(Guid tripId, Guid dayId)
        {
            var day = await _dayRepository.GetByIdAsync(dayId);
            if (day == null || day.TripId != tripId) return false;

            await _dayRepository.DeleteAsync(day);
            return true;
        }

        private TripDto MapToDto(Trip trip)
        {
            return new TripDto
            {
                Id = trip.Id,
                Title = trip.Title,
                TripType = trip.TripType,
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
                Description = day.Description
            };
        }
    }
}
