using System.ComponentModel.DataAnnotations;

namespace SharedModels.Models
{
    public class DrugCatalog
    {
        [Key]
        public int DrugId { get; set; }
        public string BrandName { get; set; }
        public string Composition { get; set; }
        public string Form { get; set; } // e.g., "Tablet"
        public string DosageStrength { get; set; } // e.g., "500 mg"
        public string Frequency { get; set; } // e.g., "Once daily"
    }
}
