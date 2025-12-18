using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MedRecordWebApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<TestCatalog> TestCatalogs { get; set; }
        public DbSet<DrugCatalog> DrugCatalogs { get; set; }
        public DbSet<LabResults> LabResults { get; set; }
    }
}
