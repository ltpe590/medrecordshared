using Domain.Models;

public interface ITestCatalogRepository
{
    Task<TestCatalog> GetByIdAsync(int testId);
    Task<List<TestCatalog>> GetAllAsync();
    Task AddAsync(TestCatalog testCatalog);
    Task UpdateAsync(TestCatalog testCatalog);
    Task<bool> DeleteAsync(int testId);
}