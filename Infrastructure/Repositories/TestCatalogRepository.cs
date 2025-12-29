using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class TestCatalogRepository : ITestCatalogRepository
    {
        private readonly ApplicationDbContext _context;

        public TestCatalogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TestCatalog> GetByIdAsync(int testId)
        {
            return await _context.TestCatalogs
                .FirstOrDefaultAsync(t => t.TestId == testId);
        }

        public async Task<IEnumerable<TestCatalog>> GetAllAsync()
        {
            return await _context.TestCatalogs.ToListAsync();
        }

        public Task<TestCatalog> AddAsync(TestCatalog testCatalog)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(TestCatalog testCatalog)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int testId)
        {
            throw new NotImplementedException();
        }
    }
}