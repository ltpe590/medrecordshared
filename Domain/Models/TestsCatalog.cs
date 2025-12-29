using System.ComponentModel.DataAnnotations;

namespace Domain.Models
{
    public class TestCatalog
    {
        [Key]
        public int TestId { get; set; }
        public required string TestName { get; set; } // e.g., "Complete Blood Count"
        public required string TestUnit { get; set; } // e.g., "mg/dL"
        public required string NormalRange { get; set; } // e.g., "90-140"
        public virtual ICollection<LabResults> LabResults { get; set; } = new List<LabResults>();
    }
}
