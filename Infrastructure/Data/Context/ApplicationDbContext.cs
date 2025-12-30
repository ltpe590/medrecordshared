using Domain.Models;
using Infrastructure.Data.Configurations;
using Infrastructure.Data.Seeding;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // DbSets (keep these simple)
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<LabResults> LabResults { get; set; }
        public DbSet<TestCatalog> TestCatalogs { get; set; }
        public DbSet<DrugCatalog> DrugCatalogs { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new PatientConfiguration());
            modelBuilder.ApplyConfiguration(new VisitConfiguration());
            modelBuilder.ApplyConfiguration(new LabResultsConfiguration());
            modelBuilder.ApplyConfiguration(new TestCatalogConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());

            TestCatalogSeeder.SeedTestCatalogs(modelBuilder);
        }
    }
}