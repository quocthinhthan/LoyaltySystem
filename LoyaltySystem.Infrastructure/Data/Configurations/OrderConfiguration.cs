using LoyaltySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoyaltySystem.Infrastructure.Data.Configurations;

/// <summary>
/// Cấu hình cho bảng Order (Đơn hàng)
/// Quan hệ:
/// - N-1 với User (Many Orders → One Customer)
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        // 1. Đặt tên bảng
        builder.ToTable("Orders");

        // 2. Thiết lập Primary Key
        builder.HasKey(o => o.OrderId);

        // 3. Cấu hình thuộc tính OrderId (Identity - Tự tăng)
        builder.Property(o => o.OrderId)
            .ValueGeneratedOnAdd()
            .HasComment("ID đơn hàng - Identity (auto-increment)");

        // 4. Cấu hình thuộc tính CustomerId
        builder.Property(o => o.CustomerId)
            .IsRequired()
            .HasComment("ID khách hàng - FK đến bảng Users");

        // 5. Cấu hình thuộc tính StaffId
        builder.Property(o => o.StaffId)
            .IsRequired()
            .HasComment("ID nhân viên tạo đơn - FK đến bảng Users");

        // 6. Cấu hình thuộc tính Price
        builder.Property(o => o.Price)
            .IsRequired()
            .HasPrecision(18, 2) // Tổng 18 chữ số, 2 chữ số thập phân
            .HasComment("Giá trị đơn hàng (VNĐ)");

        // 7. Cấu hình thuộc tính TimeCreate
        builder.Property(o => o.TimeCreate)
            .IsRequired()
            .HasDefaultValueSql("GETDATE()")
            .HasComment("Thời gian tạo đơn hàng");

        // 8. Quan hệ N-1 với User (Customer)
        // Nhiều Order thuộc về một User
        builder.HasOne(o => o.Customer)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // 9. Indexes cho tìm kiếm nhanh
        builder.HasIndex(o => o.CustomerId)
            .HasDatabaseName("IX_Order_CustomerId");

        builder.HasIndex(o => o.StaffId)
            .HasDatabaseName("IX_Order_StaffId");

        builder.HasIndex(o => o.TimeCreate)
            .HasDatabaseName("IX_Order_TimeCreate"); // Dùng cho filter theo ngày

        // 10. Composite Index cho query phổ biến
        builder.HasIndex(o => new { o.CustomerId, o.TimeCreate })
            .HasDatabaseName("IX_Order_Customer_Time");
    }
}
