using LoyaltySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoyaltySystem.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        // 1. Constructor: Cực kỳ quan trọng để Dependency Injection hoạt động
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {
        }

        // 2. Các bảng trong Database
        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<MonthlyPoints> MonthlyPoints { get; set; }

        // 3. Fluent API: Nơi thiết lập các "luật chơi" cho Database
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình bảng Account: Lấy PhoneNumber làm khóa chính
            modelBuilder.Entity<Account>()
                .HasKey(a => a.PhoneNumber);

            // Cấu hình bảng MonthlyPoints: Khai báo Composite Key (Khóa chính kết hợp)
            // Đây là phần quan trọng nhất để làm Ranking Dashboard theo tháng
            modelBuilder.Entity<MonthlyPoints>()
                .HasKey(mp => new { mp.CustomerId, mp.Month, mp.Year });

            // Thiết lập quan hệ 1-N giữa User và Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CustomerId);
            modelBuilder.Entity<Order>()
                .Property(o => o.Price)
                .HasPrecision(18, 2);
        }
    }
}
