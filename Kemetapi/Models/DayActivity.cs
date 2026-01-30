using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Kemet_api.Models
{
    public class DayActivity
    {
        public Guid Id { get; set; }

        public Guid DayId { get; set; }
        
        [JsonIgnore]
        [ForeignKey("DayId")]
        public Day? Day { get; set; }

        public Guid DestinationId { get; set; }

        [ForeignKey("DestinationId")]
        public Destination? Destination { get; set; }

        [Required]
        public ActivityType ActivityType { get; set; }

        public TimeSpan StartTime { get; set; }

        public double DurationHours { get; set; }

        public string? Description { get; set; }
    }

    public enum ActivityType
    {
        Sightseeing,
        Food,
        Museum,
        Adventure,
        Relaxation,
        Shopping,
        NightLife,
        Other
    }
}
