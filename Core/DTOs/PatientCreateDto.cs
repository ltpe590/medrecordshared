using System.ComponentModel.DataAnnotations;

namespace Core.DTOs
{
    public sealed class PatientCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; init; } = string.Empty;

        public string? Sex { get; init; }

        // EITHER age OR date of birth must be supplied.
        public int? Age { get; init; }

        public DateOnly? DateOfBirth { get; init; }

        public string? BloodGroup { get; init; }
        public string? Allergies { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Address { get; init; }
        public string? ShortNote { get; init; }
    }
}