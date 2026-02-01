using Kemet_api.Models;

namespace Kemet_api.Repositories
{
    public interface ITripRepository : IRepository<Trip>
    {
        Task<Trip?> GetTripWithDaysAsync(Guid id);
        Task<Trip?> GetUserTripWithDaysAsync(Guid tripId, Guid userId);
        Task<IEnumerable<Trip>> GetAllWithDaysAsync();
        Task<IEnumerable<Trip>> GetTripsByUserIdAsync(Guid userId);
    }
}
