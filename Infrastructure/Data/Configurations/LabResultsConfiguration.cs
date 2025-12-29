// Infrastructure/Data/Context/Configurations/LabResultsConfiguration.cs
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Context.Configurations
{
    public class LabResultsConfiguration : IEntityTypeConfiguration<LabResults>
    {
        public void Configure(EntityTypeBuilder<LabResults> builder)
        {
            builder.ToTable("LabResults");
            builder.HasKey(e => e.LabId);

            // Properties
            builder.Property(e => e.ResultValue)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(e => e.VisitId)
                   .IsRequired();

            builder.Property(e => e.TestId)
                   .IsRequired();

            builder.Property(e => e.CreatedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(l => l.Visit)
                   .WithMany(v => v.LabResults)
                   .HasForeignKey(l => l.VisitId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(l => l.TestCatalog)
                   .WithMany(tc => tc.LabResults)
                   .HasForeignKey(l => l.TestId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}