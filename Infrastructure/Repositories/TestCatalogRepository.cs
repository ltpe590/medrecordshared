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

        public async Task<List<TestCatalog>> GetAllAsync()
        {
            return await _context.TestCatalogs.ToListAsync();
        }

        public async Task AddAsync(TestCatalog testCatalog)
        {
            await _context.TestCatalogs.AddAsync(testCatalog);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TestCatalog testCatalog)
        {
            _context.TestCatalogs.Update(testCatalog);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int testId)
        {
            var test = await GetByIdAsync(testId);
            if (test == null) return false;

            _context.TestCatalogs.Remove(test);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}