using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Visit
    {
        public int VisitId { get; set; } // Primary Key
        public DateTime DateOfVisit { get; set; } = DateTime.Now;

        public string Diagnosis { get; set; } // Supports Arabic
        public string Notes { get; set; }     // Supports Arabic

        // Vital Signs & Metrics
        public decimal Temperature { get; set; }
        public int BloodPressureSystolic { get; set; }
        public int BloodPressureDiastolic { get; set; }

        // --- Patient Status at time of this Visit (Moved here from Patient.cs) ---
        public int Gravida { get; set; }
        public int Para { get; set; }
        public int Abortion { get; set; }

        // --- Pregnancy Fields (LMP/EDD) ---
        [DataType(DataType.Date)]
        public DateTime? LMP { get; set; } // Last Menstrual Period (Nullable)
        [DataType(DataType.Date)]
        public DateTime? EDD { get; set; } // Estimated Due Date (Calculated field, nullable)

        // --- Foreign Keys & Navigation Properties ---
        public int PatientId { get; set; }

        public Patient Patient { get; set; } // Navigation property

        // Links to the separate Prescriptions and LabResults table
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<LabResults> LabResults { get; set; } = new List<LabResults>();
    }
}
