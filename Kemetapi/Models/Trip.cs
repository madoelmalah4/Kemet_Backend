using System.ComponentModel.DataAnnotations;

namespace Kemet_api.Models
{
    public class Trip
    {
        public Guid Id { get; set; }
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
            
        public ICollection<Day> Days { get; set; } = new List<Day>();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum TripType
    {
        Single = 0,
        Family = 1,
        Couple = 2,
        Group = 3
    }
}
