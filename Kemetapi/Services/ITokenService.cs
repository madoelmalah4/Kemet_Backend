using Kemet_api.Models;

namespace Kemet_api.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        Task<bool> ValidateRefreshTokenAsync(User user, string refreshToken);
        Task SaveRefreshTokenAsync(User user, string refreshToken);
        Task RevokeRefreshTokenAsync(User user);
    }
}
