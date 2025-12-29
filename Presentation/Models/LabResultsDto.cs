using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // ✅ This belongs here
using System.ComponentModel.DataAnnotations;

namespace Presentation.Models
{
    public class LabResultsDto
    {
        public int LabId { get; set; }

        [Required]
        public int TestId { get; set; }

        [Required]
        public string ResultValue { get; set; }

        public int VisitId { get; set; }

        [ValidateNever] // ✅ ASP.NET Core specific - belongs here
        public VisitDto Visit { get; set; }

        [ValidateNever] // ✅ ASP.NET Core specific - belongs here
        public TestCatalogDto TestCatalog { get; set; }
    }
}