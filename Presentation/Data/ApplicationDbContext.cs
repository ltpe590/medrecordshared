using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Infrastructure.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<LabResults> LabResults { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships here instead of data annotations
            modelBuilder.Entity<LabResults>()
                .HasOne(l => l.Visit)
                .WithMany(v => v.LabResults)
                .HasForeignKey(l => l.VisitId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LabResults>()
                .HasOne(l => l.TestCatalog)
                .WithMany(tc => tc.LabResults)
                .HasForeignKey(l => l.TestId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure required relationships
            modelBuilder.Entity<LabResults>()
                .Property(l => l.VisitId)
                .IsRequired();

            modelBuilder.Entity<LabResults>()
                .Property(l => l.TestId)
                .IsRequired();
        }
    }
}