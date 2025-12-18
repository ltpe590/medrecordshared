using System.ComponentModel.DataAnnotations; // <-- Add this using statement if missing

namespace SharedModels.Models
{
    public class TestCatalog
    {
        [Key] // <-- Add this attribute explicitly
        public int TestId { get; set; } // Primary Key

        public string Name { get; set; } // e.g., "Complete Blood Count"
        public string StandardUnit { get; set; } // e.g., "mg/dL"
        public string StandardRange { get; set; } // e.g., "90-140"
    }
}
