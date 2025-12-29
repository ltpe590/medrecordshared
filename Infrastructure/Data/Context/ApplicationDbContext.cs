using Domain.Models;
using Infrastructure.Data.Context.Configurations;
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

            // Just configure entities directly for now
            modelBuilder.Entity<Patient>().ToTable("Patients");
            modelBuilder.Entity<Visit>().ToTable("Visits");
            modelBuilder.Entity<LabResults>().ToTable("LabResults");
            modelBuilder.Entity<TestCatalog>().ToTable("TestCatalogs");
            modelBuilder.Entity<DrugCatalog>().ToTable("DrugCatalogs");
            modelBuilder.Entity<Prescription>().ToTable("Prescriptions");
            modelBuilder.Entity<User>().ToTable("Users");
        }
    }
}