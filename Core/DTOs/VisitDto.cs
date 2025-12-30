using System.ComponentModel.DataAnnotations;

namespace Core.DTOs
{
    public class VisitDto
    {
        public int VisitId { get; set; }
        public int PatientId { get; set; }
        public DateTime DateOfVisit { get; set; }

        [StringLength(500)]
        public required string Diagnosis { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Notes { get; set; } = string.Empty;

        // --- Vital signs ---
        public decimal Temperature { get; set; }
        public int BloodPressureSystolic { get; set; }
        public int BloodPressureDiastolic { get; set; }

        // --- GPA ---
        public int Gravida { get; set; }
        public int Para { get; set; }
        public int Abortion { get; set; }

        // --- Pregnancy info ---
        public PregnancyInfoDto? Pregnancy { get; set; }

        /// <summary>Optional list of related prescription IDs.</summary>
        public IReadOnlyList<int> PrescriptionIds { get; set; } = Array.Empty<int>();

        /// <summary>Optional list of related lab-result IDs.</summary>
        public IReadOnlyList<int> LabResultIds { get; set; } = Array.Empty<int>();
    }

    public class PregnancyInfoDto
    {
        [DataType(DataType.Date)]
        public DateOnly? LMP { get; set; }
        public DateOnly? EDD => LMP?.AddDays(280);
    }
}