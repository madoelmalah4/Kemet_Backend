using System;
using System.ComponentModel.DataAnnotations;

namespace Kemet_api.Models
{
    public class AnalyticsEvent
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string EventType { get; set; } = string.Empty; // e.g., "FeatureUsage", "DestinationView"

        [MaxLength(100)]
        public string? Category { get; set; } // e.g., "Chatbot", "VR Tours", "DestinationName"

        public Guid? UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
