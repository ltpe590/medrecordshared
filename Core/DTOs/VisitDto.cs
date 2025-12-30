using System.ComponentModel.DataAnnotations;

namespace Core.DTOs
{
    /// <summary>
    /// Data-transfer object for a patient visit.
    /// </summary>
    public class VisitDto
    {
        public int VisitId { get; set; }

        /// <summary>Date+time of the visit (UTC).</summary>
        public DateTime DateOfVisit { get; set; }

        [Required]
        [StringLength(500)]
        public string Diagnosis { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Notes { get; set; } = string.Empty;

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

        // --- Foreign-key IDs (no navigation props) ---
        public int PatientId { get; set; }

        /// <summary>Optional list of related prescription IDs.</summary>
        public IReadOnlyList<int> PrescriptionIds { get; set; } = Array.Empty<int>();

        /// <summary>Optional list of related lab-result IDs.</summary>
        public IReadOnlyList<int> LabResultIds { get; set; } = Array.Empty<int>();
    }

    /// <summary>
    /// Nested DTO for pregnancy data.
    /// </summary>
    public class PregnancyInfoDto
    {
        [DataType(DataType.Date)]
        public DateOnly? LMP { get; set; }

        /// <summary>Estimated delivery date, computed from LMP.</summary>
        public DateOnly? EDD => LMP?.AddDays(280);
    }
}