// Infrastructure/Data/Context/Configurations/VisitConfiguration.cs
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class VisitConfiguration : IEntityTypeConfiguration<Visit>
    {
        public void Configure(EntityTypeBuilder<Visit> builder)
        {
            builder.ToTable("Visits");
            builder.HasKey(e => e.VisitId);

            builder.Property(e => e.DateOfVisit)
                   .IsRequired();

            builder.Property(e => e.Diagnosis)
                   .HasMaxLength(500);

            // Relationships
            builder.HasOne(v => v.Patient)
                   .WithMany(p => p.Visits)
                   .HasForeignKey(v => v.PatientId)
                   .OnDelete(DeleteBehavior.Cascade);

            // LabResults relationship is configured in LabResultsConfiguration
            // Prescriptions relationship is configured in PrescriptionConfiguration
        }
    }
}