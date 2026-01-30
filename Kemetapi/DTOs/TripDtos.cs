using Kemet_api.Models;
using System.ComponentModel.DataAnnotations;

namespace Kemet_api.DTOs
{
    public class CreateTripDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public TravelCompanion TravelCompanions { get; set; }

        [Required]
        public TravelStyle TravelStyle { get; set; }

        public List<string> ExperienceTypes { get; set; } = new();

        public List<string> Interests { get; set; } = new();

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int DurationDays { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public List<CreateDayDto>? Days { get; set; }
    }

    public class UpdateTripDto : CreateTripDto
    {
    }

    public class TripDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        
        public TravelCompanion TravelCompanions { get; set; }
        public TravelStyle TravelStyle { get; set; }
        public List<string> ExperienceTypes { get; set; } = new();
        public List<string> Interests { get; set; } = new();

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DurationDays { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public ICollection<DayDto> Days { get; set; } = new List<DayDto>();
        public DateTime CreatedAt { get; set; }
    }

    public class CreateDayDto
    {
        [Required]
        public int DayNumber { get; set; }

        public DateTime? Date { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string City { get; set; } = string.Empty;
    }

    public class UpdateDayDto : CreateDayDto
    {
    }

    public class DayDto
    {
        public Guid Id { get; set; }
        public Guid TripId { get; set; }
        public int DayNumber { get; set; }
        public DateTime? Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public ICollection<DayActivityDto> Activities { get; set; } = new List<DayActivityDto>();
    }

    public class CreateDayActivityDto
    {
        [Required]
        public Guid DestinationId { get; set; }

        [Required]
        public ActivityType ActivityType { get; set; }

        public TimeSpan StartTime { get; set; }

        public double DurationHours { get; set; }

        public string? Description { get; set; }
    }

    public class UpdateDayActivityDto : CreateDayActivityDto
    {
    }

    public class DayActivityDto
    {
        public Guid Id { get; set; }
        public Guid DayId { get; set; }
        public Guid DestinationId { get; set; }
        public string DestinationName { get; set; } = string.Empty; // Useful for UI
        public string? DestinationImageUrl { get; set; }
        public ActivityType ActivityType { get; set; }
        public TimeSpan StartTime { get; set; }
        public double DurationHours { get; set; }
        public string? Description { get; set; }
    }
}
