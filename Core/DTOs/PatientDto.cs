using System;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs
{
    public sealed class PatientDto
    {
        public int PatientId { get; init; }

        [Required, StringLength(100)]
        public string Name { get; init; } = string.Empty;

        public string? Sex { get; init; }
        public DateTime DateOfBirth { get; init; }

        public string? BloodGroup { get; init; }
        public string? Allergies { get; init; }
        public string? PhoneNumber { get; init; }
        public string? Address { get; init; }
        public string? ShortNote { get; init; }
    }
}