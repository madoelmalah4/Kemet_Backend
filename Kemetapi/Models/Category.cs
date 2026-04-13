using System.ComponentModel.DataAnnotations;

namespace Kemet_api.Models
{
    public class Category
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        public ICollection<Destination> Destinations { get; set; } = new List<Destination>();
    }
}
