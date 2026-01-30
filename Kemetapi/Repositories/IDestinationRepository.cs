using Kemet_api.Models;

namespace Kemet_api.Repositories
{
    public interface IDestinationRepository : IRepository<Destination>
    {
        Task<IEnumerable<Destination>> GetAllWithVirtualTourAsync();
        Task<Destination?> GetByIdWithVirtualTourAsync(Guid id);
        Task AddFavoriteAsync(UserFavorite favorite);
        Task RemoveFavoriteAsync(UserFavorite favorite);
        Task<UserFavorite?> GetFavoriteAsync(Guid userId, Guid destinationId);
        Task<IEnumerable<Destination>> GetFavoritesByUserIdAsync(Guid userId);
    }
}
