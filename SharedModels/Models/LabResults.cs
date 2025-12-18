using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Newtonsoft.Json;
using SharedModels.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SharedModels.Models
{
    public class LabResults
    {
        [Key]
        public int LabId { get; set; }
        public int TestId { get; set; }
        public string ResultValue { get; set; }

        // Navigation properties for EF Core:
        public int VisitId { get; set; }
        [ValidateNever]
        [JsonIgnore]
        public Visit Visit { get; set; }

        // Use this to pull the TestName, Unit, and Range from the master list:
        [ForeignKey("TestId")]
        [ValidateNever]
        [JsonIgnore]
        public TestCatalog TestCatalog { get; set; }
    }
}