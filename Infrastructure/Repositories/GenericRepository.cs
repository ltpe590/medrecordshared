using Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Core.Interfaces.Repositories;

namespace Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<T>> GetAllAsync() => await _context.Set<T>().ToListAsync();
        public async Task<T?> GetByIdAsync(int id) => await _context.Set<T>().FindAsync(id);
        public async Task AddAsync(T entity) => await _context.Set<T>().AddAsync(entity);
        public async Task UpdateAsync(T entity) => _context.Set<T>().Update(entity);
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            _context.Set<T>().Remove(entity);
            return true;
        }
        public async Task<bool> ExistsAsync(int id) => await GetByIdAsync(id) != null;

        Task<IEnumerable<T>> IGenericRepository<T>.GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public void Update(T entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(T entity)
        {
            throw new NotImplementedException();
        }
    }
}