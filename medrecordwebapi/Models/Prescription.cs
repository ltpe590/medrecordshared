using MedRecordWebApi.Models;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

// ... (using statements from before)
public class Prescription
{
    public int PrescriptionId { get; set; }
    // We now link to the master list DrugId:
    public int DrugId { get; set; }
    public string Dosage { get; set; } // e.g., "1 tablet twice daily"
    public string DurationDays { get; set; }

    // Navigation properties for EF Core:
    public int VisitId { get; set; }
    [Validatenever]
    [JsonIgnore]
    public Visit Visit { get; set; }
    // Use this to pull the Brand/Generic name from the master list:
    [ForeignKey("DrugId")]
    [ValidateNever]
    [JsonIgnore]
    public DrugCatalog DrugCatalog { get; set; }
}
