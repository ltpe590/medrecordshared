// Infrastructure/Data/Context/Configurations/TestCatalogConfiguration.cs
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Context.Configurations
{
    public class TestCatalogConfiguration : IEntityTypeConfiguration<TestCatalog>
    {
        public void Configure(EntityTypeBuilder<TestCatalog> builder)
        {
            builder.ToTable("TestCatalogs");
            builder.HasKey(e => e.TestId);

            builder.Property(e => e.TestName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.TestUnit)
                   .HasMaxLength(50);

            builder.Property(e => e.NormalRange)
                   .HasMaxLength(100);

            // Index for performance
            builder.HasIndex(e => e.TestName)
                   .IsUnique();
        }
    }
}