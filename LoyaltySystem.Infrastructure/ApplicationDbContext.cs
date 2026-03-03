using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoyaltySystem.Infrastructure
{
    public class ApplicationDbContext : DbContext, IAppDbContext
    {
        // 1. Constructor
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // 2. Các bảng trong Database (DbSet của EF Core)
        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<MonthlyPoints> MonthlyPoints { get; set; }

        // --- BỔ SUNG ĐỂ THỰC THI INTERFACE IAppDbContext ---

        // Ánh xạ các DbSet sang IQueryable để tầng Application có thể truy vấn
        IQueryable<Account> IAppDbContext.Accounts => Accounts;
        IQueryable<User> IAppDbContext.Users => Users;
        IQueryable<Order> IAppDbContext.Orders => Orders;
        IQueryable<MonthlyPoints> IAppDbContext.MonthlyPoints => MonthlyPoints;

        // Triển khai các phương thức thao tác thực thể dùng chung (Generic)
        public void AddEntity<T>(T entity) where T : class
        {
            Set<T>().Add(entity);
        }

        public void UpdateEntity<T>(T entity) where T : class
        {
            Set<T>().Update(entity);
        }

        public void RemoveEntity<T>(T entity) where T : class
        {
            Set<T>().Remove(entity);
        }

        // SaveChangesAsync đã có sẵn trong DbContext nên không cần viết lại nội dung, 
        // chỉ cần đảm bảo Interface khớp chữ ký hàm là đủ.

        // --- HẾT PHẦN BỔ SUNG ---

        // 3. Fluent API: Nơi thiết lập các "luật chơi" cho Database
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình bảng Account: Lấy PhoneNumber làm khóa chính
            modelBuilder.Entity<Account>()
                .HasKey(a => a.PhoneNumber);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);

                // Cấu hình quan hệ 1-1 giữa User và Account
                entity.HasOne(u => u.Account)
                      .WithOne(a => a.User)
                      .HasForeignKey<Account>(a => a.PhoneNumber)
                      .HasPrincipalKey<User>(u => u.PhoneNumber);
            });

            // Cấu hình bảng MonthlyPoints: Khai báo Composite Key (Khóa chính kết hợp)
            modelBuilder.Entity<MonthlyPoints>()
                .HasKey(mp => new { mp.CustomerId, mp.Month, mp.Year });

            // Thiết lập quan hệ 1-N giữa User (Customer) và Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CustomerId);

            // Cấu hình độ chính xác cho cột giá tiền
            modelBuilder.Entity<Order>()
                .Property(o => o.Price)
                .HasPrecision(18, 2);
        }
    }
}