using System.ComponentModel.DataAnnotations;

namespace Kemet_api.Models
{
    public class Destination
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public decimal EstimatedPrice { get; set; }

        public TimeSpan? FromWorkingHours { get; set; }

        public TimeSpan? EndWorkingHours { get; set; }

        public VirtualTour? VirtualTour { get; set; }
    }
}
