using Kemet_api.Data;
using Kemet_api.Models;
using Microsoft.EntityFrameworkCore;

namespace Kemet_api.Repositories
{
    public class DestinationRepository : Repository<Destination>, IDestinationRepository
    {
        public DestinationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Destination>> GetAllWithVirtualTourAsync()
        {
            return await _context.Destinations
                .Include(d => d.VirtualTour)
                .ToListAsync();
        }

        public async Task<Destination?> GetByIdWithVirtualTourAsync(Guid id)
        {
            return await _context.Destinations
                .Include(d => d.VirtualTour)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddFavoriteAsync(UserFavorite favorite)
        {
            await _context.UserFavorites.AddAsync(favorite);
        }

        public async Task RemoveFavoriteAsync(UserFavorite favorite)
        {
            _context.UserFavorites.Remove(favorite);
            await Task.CompletedTask;
        }

        public async Task<UserFavorite?> GetFavoriteAsync(Guid userId, Guid destinationId)
        {
            return await _context.UserFavorites
                .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.DestinationId == destinationId);
        }

        public async Task<IEnumerable<Destination>> GetFavoritesByUserIdAsync(Guid userId)
        {
            return await _context.Destinations
                .Where(d => _context.UserFavorites.Any(uf => uf.UserId == userId && uf.DestinationId == d.Id))
                .Include(d => d.VirtualTour)
                .ToListAsync();
        }
    }
}
