using Core.Helpers;
using System;

namespace WPF.ViewModels
{
    public sealed class PatientViewModel
    {
        public int PatientId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Sex { get; init; }   // from DTO
        public DateTime DateOfBirth { get; init; }   // DateOnly → DateTime for WPF binding
        public string? PhoneNumber { get; init; }
        public string? Address { get; init; }

        public string DisplayName => Name;
        public int Age => AgeCalculator.FromDateOfBirth(DateOfBirth);
    }
}