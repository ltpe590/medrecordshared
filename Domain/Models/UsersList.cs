using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public required string Username { get; set; }

        [StringLength(100)]
        public required string Email { get; set; }

        [StringLength(20)]
        public required string PhoneNumber { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        // For fingerprint authentication
        public bool HasFingerprintEnrolled { get; set; }
        public byte[] FingerprintTemplate { get; set; } = Array.Empty<byte>();

    }
}