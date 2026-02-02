using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Kemet_api.Models;
using Kemet_api.Repositories;

namespace Kemet_api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly string _pepper;

        public AuthService(IUserRepository userRepository, ITokenService tokenService, IEmailService emailService, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _emailService = emailService;
            _pepper = configuration["PasswordSettings:Pepper"] ?? "default-pepper";
        }

        public async Task<(User? user, string? message)> RegisterAsync(string email, string password, string firstName, string lastName, UserRole role = UserRole.User)
        {
            if (await UserExistsAsync(email))
            {
                return (null, "Email already exists");
            }

            var otp = GenerateRandomOtp();
            var user = new User
            {
                Email = email,
                PasswordHash = HashPassword(password),
                FirstName = firstName,
                LastName = lastName,
                Role = role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsEmailVerified = false,
                OtpCode = otp,
                OtpExpiry = DateTime.UtcNow.AddMinutes(10)
            };

            var createdUser = await _userRepository.AddAsync(user);
            
            try 
            {
                await _emailService.SendOtpEmailAsync(email, otp);
            }
            catch (Exception ex)
            {
                // In production, log this correctly
                return (createdUser, "User registered but failed to send verification email: " + ex.Message);
            }

            return (createdUser, "User registered successfully. Please check your email for verification code.");
        }

        public async Task<(User? user, string? message)> LoginAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null || !user.IsActive)
            {
                return (null, "Invalid credentials");
            }

            if (!user.IsEmailVerified)
            {
                return (null, "Please verify your email address before logging in.");
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                return (null, "Invalid credentials");
            }

            return (user, "Login successful");
        }

        public async Task<(bool success, string message)> VerifyEmailAsync(string email, string otp)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return (false, "User not found");

            if (user.IsEmailVerified) return (false, "Email is already verified");
            if (user.OtpCode != otp) return (false, "Invalid verification code");
            if (user.OtpExpiry < DateTime.UtcNow) return (false, "Verification code has expired");

            user.IsEmailVerified = true;
            user.OtpCode = null;
            user.OtpExpiry = null;

            await _userRepository.UpdateAsync(user);
            return (true, "Email verified successfully");
        }

        public async Task<(bool success, string message)> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return (false, "If an account exists with this email, you will receive a reset code.");

            var otp = GenerateRandomOtp();
            user.OtpCode = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);

            await _userRepository.UpdateAsync(user);
            try 
            {
                await _emailService.SendOtpEmailAsync(email, otp);
            }
            catch (Exception ex)
            {
                return (false, "Verification record created, but failed to send email: " + ex.Message);
            }

            return (true, "Reset code sent to your email.");
        }

        public async Task<(bool success, string message)> ResetPasswordAsync(string email, string otp, string newPassword)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return (false, "User not found");

            if (user.OtpCode != otp) return (false, "Invalid reset code");
            if (user.OtpExpiry < DateTime.UtcNow) return (false, "Reset code has expired");

            user.PasswordHash = HashPassword(newPassword);
            user.OtpCode = null;
            user.OtpExpiry = null;
            user.IsEmailVerified = true; // Naturally verified if they can reset via email

            await _userRepository.UpdateAsync(user);
            return (true, "Password has been reset successfully.");
        }

        public async Task<(bool success, string message)> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return (false, "User not found");

            if (!VerifyPassword(currentPassword, user.PasswordHash))
            {
                return (false, "Current password is incorrect");
            }

            user.PasswordHash = HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);
            return (true, "Password changed successfully");
        }

        public async Task<(bool success, string message)> ResendOtpAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return (false, "User not found");

            if (user.IsEmailVerified) return (false, "Email is already verified");

            var otp = GenerateRandomOtp();
            user.OtpCode = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);

            await _userRepository.UpdateAsync(user);
            try 
            {
                await _emailService.SendOtpEmailAsync(email, otp);
            }
            catch (Exception ex)
            {
                return (false, "Verification code generated, but failed to send email: " + ex.Message);
            }

            return (true, "New verification code sent to your email.");
        }

        public async Task<(string? accessToken, string? refreshToken, string? message)> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
            if (user == null)
            {
                return (null, null, "Invalid refresh token");
            }

            var isValidRefreshToken = await _tokenService.ValidateRefreshTokenAsync(user, refreshToken);
            if (!isValidRefreshToken)
            {
                return (null, null, "Invalid refresh token");
            }

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            await _tokenService.SaveRefreshTokenAsync(user, newRefreshToken);

            return (newAccessToken, newRefreshToken, "Token refreshed successfully");
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
            if (user == null)
            {
                return false;
            }

            await _tokenService.RevokeRefreshTokenAsync(user);
            return true;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            var existingEmail = await _userRepository.GetByEmailAsync(email);
            return existingEmail != null;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<(bool success, string message)> UpdateUserRoleAsync(Guid userId, UserRole newRole)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return (false, "User not found");

            user.Role = newRole;
            await _userRepository.UpdateAsync(user);
            return (true, $"User role updated to {newRole} successfully");
        }

        private string GenerateRandomOtp()
        {
            return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        }

        private string HashPassword(string password, string? salt = null)
        {
            salt ??= GenerateSalt();
            var saltedPassword = password + salt + _pepper;
            
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(saltedPassword));
            return salt + Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var salt = storedHash.Length > 44 ? storedHash.Substring(0, 44) : "";
            var computedHash = HashPassword(password, salt);
            return storedHash == computedHash;
        }

        private string GenerateSalt()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
