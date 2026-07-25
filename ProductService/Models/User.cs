using System.ComponentModel.DataAnnotations;

namespace ProductService.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<Wishlist> Wishlists { get; set; }
    = new List<Wishlist>();

        public string? RefreshToken { get; set; }

        public DateTime RefreshTokenExpiryTime { get; set; }
        public string? PasswordResetToken { get; set; }

        public DateTime? ResetTokenExpires { get; set; }
        public bool EmailConfirmed { get; set; } = false;

        public string? EmailVerificationToken { get; set; }

        public DateTime? EmailVerificationTokenExpiry { get; set; }

        public int AccessFailedCount { get; set; }

        public DateTime? LockoutEnd { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; }
    = new List<RefreshToken>();
    }
}