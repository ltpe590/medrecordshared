using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Doctor
    {
        public int DoctorId { get; set; } // Primary Key

        [Required]
        public string DoctorName { get; set; } // Combined name field (supports Arabic text)

        public string Degree { get; set; } // e.g., M.D., MBBS, Ph.D.

        public string Specialty { get; set; } // e.g., Cardiology, Dermatology

        public string ClinicName { get; set; }

        public string ClinicAddress { get; set; }

        public string ClinicPhoneNumber { get; set; }

        // --- Suggestions for future use ---
        public string Email { get; set; } // Good for system communications

        // Navigation property: EF Core uses this to efficiently load associated medical records
        public ICollection<Visit> MedicalRecords { get; set; } = new List<Visit>();
    }
}
