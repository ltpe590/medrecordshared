using Microsoft.EntityFrameworkCore;
using MedRecordWebApi.Models; // Or medrecordwebapi.Models if your project name is lowercase

namespace MedRecordWebApi.Data // Use your actual project name here
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define the tables (DbSets) for our database
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<TestCatalog> TestCatalogs { get; set; }
        public DbSet<DrugCatalog> DrugCatalogs { get; set; }
        public DbSet<LabResults> LabResults { get; set; }
    }
}
