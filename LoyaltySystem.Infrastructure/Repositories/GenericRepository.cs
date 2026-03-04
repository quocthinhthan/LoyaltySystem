using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LoyaltySystem.Domain.Interfaces;
using LoyaltySystem.Infrastructure.Data;

namespace LoyaltySystem.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
        public IQueryable<T> GetQueryable() => _dbSet.AsQueryable();
        public async Task<T?> GetByIdAsync(object id) => await _dbSet.FindAsync(id);
        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) 
            => await _dbSet.FirstOrDefaultAsync(predicate);
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) 
            => await _dbSet.Where(predicate).ToListAsync();
        public void Add(T entity) => _dbSet.Add(entity);
        public void Update(T entity) => _dbSet.Update(entity);
        public void Delete(T entity) => _dbSet.Remove(entity);

        public IQueryable<T> Query()
        {
            return _dbSet;
        }
    }
}
