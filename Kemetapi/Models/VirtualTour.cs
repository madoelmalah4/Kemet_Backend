using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kemet_api.Models
{
    public class VirtualTour
    {
        [Key]
        public Guid Vr_id { get; set; }

        public Guid Destination_id { get; set; }

        [ForeignKey("Destination_id")]
        public Destination? Destination { get; set; }

        public string? Vr_urlImage { get; set; }

        public DateTime Created_at { get; set; } = DateTime.UtcNow;
    }
}
