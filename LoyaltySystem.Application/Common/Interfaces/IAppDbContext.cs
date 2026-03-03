using LoyaltySystem.Domain.Entities;

namespace LoyaltySystem.Application.Common.Interfaces;

public interface IAppDbContext
{
    // Sử dụng IQueryable để Query dữ liệu mà không cần EF Core package
    IQueryable<Account> Accounts { get; }
    IQueryable<User> Users { get; }
    IQueryable<Order> Orders { get; }
    IQueryable<MonthlyPoints> MonthlyPoints { get; }

    // Thêm các phương thức thao tác để Command Handler sử dụng
    void AddEntity<T>(T entity) where T : class;
    void UpdateEntity<T>(T entity) where T : class;
    void RemoveEntity<T>(T entity) where T : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}