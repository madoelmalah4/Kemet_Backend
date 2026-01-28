using System.ComponentModel.DataAnnotations;

namespace Kemet_api.Models
{
    public class User
    {
        public Guid Id { get; set; }
        
        
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        public UserRole Role { get; set; } = UserRole.User;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public bool IsEmailVerified { get; set; } = false;
        public string? OtpCode { get; set; }
        public DateTime? OtpExpiry { get; set; }
        
        public string? RefreshToken { get; set; }
        
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }

    public enum UserRole
    {
        User = 0,
        Admin = 1
    }
}
