using Kemet_api.Models;

namespace Kemet_api.Services
{
    public interface IAuthService
    {
        Task<(User? user, string? message)> RegisterAsync(string email, string password, string firstName, string lastName, UserRole role = UserRole.User);
        Task<(User? user, string? message)> LoginAsync(string email, string password);
        Task<(string? accessToken, string? refreshToken, string? message)> RefreshTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(string refreshToken);
        Task<bool> UserExistsAsync(string email);
        
        // New Methods
        Task<(bool success, string message)> VerifyEmailAsync(string email, string otp);
        Task<(bool success, string message)> ForgotPasswordAsync(string email);
        Task<(bool success, string message)> ResetPasswordAsync(string email, string otp, string newPassword);
        Task<(bool success, string message)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
        Task<(bool success, string message)> ResendOtpAsync(string email);
    }
}
