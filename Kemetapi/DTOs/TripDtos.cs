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
        public TripType TripType { get; set; }

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
        public string Title { get; set; } = string.Empty;
        public TripType TripType { get; set; }
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
    }
}
