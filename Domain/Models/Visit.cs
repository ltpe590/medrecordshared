using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Visit
    {
        public int VisitId { get; set; }
        public DateTime DateOfVisit { get; set; } = DateTime.Now;

        public required string Diagnosis { get; set; }
        public required string Notes { get; set; }

        // Vital Signs & Metrics
        public decimal Temperature { get; set; }
        public int BloodPressureSystolic { get; set; }
        public int BloodPressureDiastolic { get; set; }

        // --- GPA ---
        public int Gravida { get; set; }
        public int Para { get; set; }
        public int Abortion { get; set; }

        public class PregnancyModel
        {
            [DataType(DataType.Date)]
            public DateTime? LMP { get; set; }

            [DataType(DataType.Date)]
            public DateTime? EDD => LMP?.AddDays(280);
        }

        // --- Foreign Keys & Navigation Properties ---
        public int PatientId { get; set; }

        public required Patient Patient { get; set; } // Navigation property


        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<LabResults> LabResults { get; set; } = new List<LabResults>();
    }
}
