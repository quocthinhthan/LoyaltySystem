using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoyaltySystem.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Khai báo các Repository cho từng bảng
        IGenericRepository<Entities.User> Users { get; }
        IGenericRepository<Entities.Order> Orders { get; }
        IGenericRepository<Entities.MonthlyPoints> MonthlyPoints { get; }
        IGenericRepository<Entities.Account> Account { get; }
        
        // Hàm lưu tất cả thay đổi vào DB (Transaction)
        Task<int> CompleteAsync();
    }
}
