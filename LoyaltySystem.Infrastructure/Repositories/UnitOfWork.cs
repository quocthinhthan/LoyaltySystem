using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoyaltySystem.Domain.Interfaces;

namespace LoyaltySystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IGenericRepository<Domain.Entities.User> Users { get; private set; }
        public IGenericRepository<Domain.Entities.Order> Orders { get; private set; }
        public IGenericRepository<Domain.Entities.MonthlyPoints> MonthlyPoints { get; private set; }

        public IGenericRepository<Domain.Entities.Account> Account { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new GenericRepository<Domain.Entities.User>(_context);
            Orders = new GenericRepository<Domain.Entities.Order>(_context);
            MonthlyPoints = new GenericRepository<Domain.Entities.MonthlyPoints>(_context);
            Account = new GenericRepository<Domain.Entities.Account>(_context);
        }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
