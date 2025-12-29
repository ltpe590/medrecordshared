using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // ✅ Built-in

// ... (using statements from before)
namespace Domain.Models
{
    public class Prescription
    {
        public int PrescriptionId { get; set; }
        // We now link to the master list DrugId:
        public int DrugId { get; set; }
        public string Dosage { get; set; } // e.g., "1 tablet twice daily"
        public string DurationDays { get; set; }

        // Navigation properties for EF Core:
        public int VisitId { get; set; }
        [JsonIgnore]
        public Visit Visit { get; set; }
        // Use this to pull the Brand/Generic name from the master list:
        [ForeignKey("DrugId")]
        [JsonIgnore]
        public DrugCatalog DrugCatalog { get; set; }
    }
}
