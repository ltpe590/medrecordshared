// Application/Interfaces/Repositories/ITestCatalogRepository.cs
using System.Threading.Tasks;
using Domain.Models;

namespace Application.Interfaces.Repositories
{
    public interface ITestCatalogRepository
    {
        Task<TestCatalog> GetByIdAsync(int testId);
        Task<IEnumerable<TestCatalog>> GetAllAsync();
        Task<TestCatalog> AddAsync(TestCatalog testCatalog);
        Task UpdateAsync(TestCatalog testCatalog);
        Task DeleteAsync(int testId);
    }
}