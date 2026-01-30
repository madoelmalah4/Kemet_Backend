using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kemet_api.Models
{
    public class UserFavorite
    {
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public Guid DestinationId { get; set; }
        [ForeignKey("DestinationId")]
        public Destination? Destination { get; set; }
        
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
