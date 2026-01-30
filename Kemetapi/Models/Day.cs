using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kemet_api.Models
{

    public class Day
    {
        public Guid Id { get; set; }

        public Guid TripId { get; set; }

        [JsonIgnore]
        public Trip? Trip { get; set; }

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

        public ICollection<DayActivity> DayActivities { get; set; } = new List<DayActivity>();
    }
}
