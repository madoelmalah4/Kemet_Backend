using Microsoft.EntityFrameworkCore;
using Kemet_api.Data;
using Kemet_api.Models;

namespace Kemet_api.Repositories
{
    public class TripRepository : Repository<Trip>, ITripRepository
    {
        public TripRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Trip?> GetTripWithDaysAsync(Guid id)
        {
            return await _dbSet
                .Include(t => t.Days)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        public async Task<IEnumerable<Trip>> GetAllWithDaysAsync()
        {
            return await _dbSet
                .Include(t => t.Days)
                .ToListAsync();
        }
    }
}
