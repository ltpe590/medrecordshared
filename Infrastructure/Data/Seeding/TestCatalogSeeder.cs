// Infrastructure/Data/Seeding/TestCatalogSeeder.cs
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Seeding
{
    public static class TestCatalogSeeder
    {
        public static void SeedTestCatalogs(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestCatalog>().HasData(
                new TestCatalog
                {
                    TestId = 1,
                    TestName = "Complete Blood Count",
                    TestUnit = "cells/µL",
                    NormalRange = "4.5-11.0"
                },
                new TestCatalog
                {
                    TestId = 2,
                    TestName = "Glucose Fasting",
                    TestUnit = "mg/dL",
                    NormalRange = "70-100"
                },
                new TestCatalog
                {
                    TestId = 3,
                    TestName = "Cholesterol Total",
                    TestUnit = "mg/dL",
                    NormalRange = "<200"
                }
            );
        }
    }
}