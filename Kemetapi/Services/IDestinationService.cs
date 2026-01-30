using Kemet_api.DTOs.Destination;

namespace Kemet_api.Services
{
    public interface IDestinationService
    {
        Task<IEnumerable<DestinationDto>> GetAllDestinationsAsync();
        Task<DestinationDto?> GetDestinationByIdAsync(Guid id);
        Task<DestinationDto> CreateDestinationAsync(CreateDestinationDto dto);
        Task<DestinationDto?> UpdateDestinationAsync(Guid id, UpdateDestinationDto dto);
        Task<bool> DeleteDestinationAsync(Guid id);
        Task<bool> AddToFavoritesAsync(Guid userId, Guid destinationId);
        Task<bool> RemoveFromFavoritesAsync(Guid userId, Guid destinationId);
        Task<IEnumerable<DestinationDto>> GetUserFavoritesAsync(Guid userId);
    }
}
