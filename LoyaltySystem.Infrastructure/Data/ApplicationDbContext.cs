using LoyaltySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LoyaltySystem.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
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

        // 3. Fluent API: Tự động load tất cả Entity Configurations
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tự động áp dụng tất cả các IEntityTypeConfiguration trong Assembly hiện tại
            // Tìm và load: AccountConfiguration, UserConfiguration, OrderConfiguration, MonthlyPointsConfiguration
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}