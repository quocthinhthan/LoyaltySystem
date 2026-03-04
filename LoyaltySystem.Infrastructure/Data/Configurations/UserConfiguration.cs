using LoyaltySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoyaltySystem.Infrastructure.Data.Configurations;

/// <summary>
/// Cấu hình cho bảng User (Người dùng - Customer/Staff/Admin)
/// Quan hệ:
/// - 1-1 với Account
/// - 1-N với Order (Customer → Orders)
/// - 1-N với MonthlyPoints (Customer → MonthlyPoints)
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // 1. Đặt tên bảng
        builder.ToTable("Users");

        // 2. Thiết lập Primary Key
        builder.HasKey(u => u.UserId);

        // 3. Cấu hình thuộc tính UserId (Identity - Tự tăng)
        builder.Property(u => u.UserId)
            .ValueGeneratedOnAdd()
            .HasComment("ID người dùng - Identity (auto-increment)");

        // 4. Cấu hình thuộc tính UserName
        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Tên người dùng");

        // 5. Cấu hình thuộc tính PhoneNumber
        builder.Property(u => u.PhoneNumber)
            .IsRequired()
            .HasMaxLength(15)
            .HasComment("Số điện thoại - Unique, FK đến Account");

        // 6. Cấu hình thuộc tính Role
        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Customer")
            .HasComment("Vai trò: Admin, Staff, Customer");

        // 7. Cấu hình thuộc tính TotalPoint
        builder.Property(u => u.TotalPoint)
            .HasDefaultValue(0)
            .HasComment("Tổng điểm tích lũy trọn đời (dùng cho ranking)");

        // 8. Quan hệ 1-1 với Account
        // (Đã cấu hình ở AccountConfiguration)

        // 9. Quan hệ 1-N với Order (User có nhiều Orders)
        builder.HasMany(u => u.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
