using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models
{
    public class LabResults
    {
        [Key]
        public int LabId { get; set; }

        [Required]
        public int TestId { get; set; }

        [StringLength(500)]
        public string ResultValue { get; set; } = string.Empty;

        [Required]
        public int VisitId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Visit? Visit { get; set; }

        [ForeignKey("TestId")]
        public virtual required TestCatalog TestCatalog { get; set; }
    }
}