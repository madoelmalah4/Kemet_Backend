using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kemet_api.Models
{
    public class Trip
    {
        public Guid Id { get; set; }

        public Guid? UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        // Merged from Questionnaire: Replaces old TripType
        [Required]
        public TravelCompanion TravelCompanions { get; set; }

        // Merged from Questionnaire
        [Required]
        public TravelStyle TravelStyle { get; set; }

        // Merged from Questionnaire: Stored as JSON
        public List<string> ExperienceTypes { get; set; } = new();

        // Merged from Questionnaire: Stored as JSON
        public List<string> Interests { get; set; } = new();

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
    public enum TravelCompanion
    {
        Solo,
        Couple,
        Family,
        Friends
    }

    public enum TravelStyle
    {
        Budget,
        MidBudget,
        Luxury
    }
}
