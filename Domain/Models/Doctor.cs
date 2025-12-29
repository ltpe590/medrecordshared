using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required]
        public required string DoctorName { get; set; }

        public required string Degree { get; set; } // M.b.Ch.B, Ph.D.

        public required string Specialty { get; set; } // Cardiology, Dermatology

        public required string ClinicName { get; set; }

        public required string ClinicAddress { get; set; }

        public required string ClinicPhoneNumber { get; set; }

        public string? Email { get; set; } 

        // Navigation property
        public ICollection<Visit> MedicalRecords { get; set; } = new List<Visit>();
    }
}
