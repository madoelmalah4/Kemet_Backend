using System.ComponentModel.DataAnnotations;

namespace Kemet_api.DTOs.Destination
{
    public class DestinationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal EstimatedPrice { get; set; }
        public TimeSpan? FromWorkingHours { get; set; }
        public TimeSpan? EndWorkingHours { get; set; }
        public Guid? VrId { get; set; }
        public string? VrUrlImage { get; set; }
    }
}
