using System;
using System.Collections.Generic; // Required for ICollection
using System.ComponentModel.DataAnnotations; // Required for [Required] and [DataType]

namespace SharedModels.Models
{
    public class Patient
    {
        // Primary Key for the database table
        public int PatientId { get; set; }

        public string Name { get; set; } // Single combined name field

        public string Sex { get; set; } // Stored as a string (e.g., "Male", "Female")

        [DataType(DataType.Date)] // Hint for UI, stored as DateTime in DB
        public DateTime DateOfBirth { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; } // Stored as a string, UI logic handles dropdown

        public string ShortNote { get; set; }

        // Navigation property: EF Core uses this to efficiently load associated medical records
        public ICollection<Visit> MedicalRecords { get; set; } = new List<Visit>();
    }
}
