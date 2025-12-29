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

        [Required]
        [StringLength(500)]
        public string ResultValue { get; set; }

        [Required]
        public int VisitId { get; set; }

        // Navigation properties - EF Core handles these
        public virtual Visit Visit { get; set; }

        [ForeignKey("TestId")]
        public virtual TestCatalog TestCatalog { get; set; }
    }
}