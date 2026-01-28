using Microsoft.EntityFrameworkCore;
using Kemet_api.Data;
using Kemet_api.Models;

namespace Kemet_api.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }


        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && 
                                         u.RefreshTokenExpiryTime > DateTime.UtcNow);
        }
    }
}
