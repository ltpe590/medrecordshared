using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class Patient
    {
        public int PatientId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Sex { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        public int Age => DateTime.Now.Year - DateOfBirth.Year;
        public string BloodGroup { get; set; }
        public string? Allergies { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? ShortNote { get; set; }

        public ICollection<Visit> MedicalRecords { get; set; } = new List<Visit>();
    }
    public static class AgeConverter
    {
        public static DateTime AgeToDateOfBirth(int age)
        {
            return DateTime.Today.AddYears(-age);
        }

        public static int DateOfBirthToAge(DateTime dateOfBirth)
        {
            return DateTime.Now.Year - dateOfBirth.Year;
        }
    }
}
