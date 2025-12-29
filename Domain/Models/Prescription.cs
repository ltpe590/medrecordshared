using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Models
{
    public class Prescription
    {
        public int PrescriptionId { get; set; }
        public required int DrugId { get; set; }
        public string? Dosage { get; set; } // e.g., "1 tablet twice daily"
        public string? DurationDays { get; set; }

        // Navigation properties
        public int VisitId { get; set; }
        public required Visit Visit { get; set; }
        
        [ForeignKey("DrugId")]
        
        public required DrugCatalog DrugCatalog { get; set; }
    }
}
