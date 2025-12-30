using System.ComponentModel.DataAnnotations;

namespace Core.DTOs
{
    /// <summary>
    /// Flat data-transfer object for patient information.
    /// </summary>
    public class PatientDto
    {
        public int PatientId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Sex { get; set; }

        /// <summary>
        /// Date of birth as UTC DateOnly (yyyy-MM-dd).
        /// </summary>
        [DataType(DataType.Date)]
        public DateOnly DateOfBirth { get; set; }

        /// <summary>
        /// Computed age (no setter – calculated on the fly).
        /// </summary>
        public int Age => DateTime.UtcNow.Year - DateOfBirth.Year;

        [StringLength(5)]
        public string? BloodGroup { get; set; }

        [StringLength(500)]
        public string? Allergies { get; set; }

        [Phone]
        [StringLength(25)]
        public string? PhoneNumber { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(500)]
        public string? ShortNote { get; set; }
    }
}