using LoyaltySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoyaltySystem.Infrastructure.Data.Configurations;

/// <summary>
/// Cấu hình cho bảng MonthlyPoints (Điểm tích lũy theo tháng)
/// Quan hệ:
/// - N-1 với User (Many MonthlyPoints → One Customer)
/// Đặc biệt: Sử dụng Composite Key (CustomerId, Month, Year)
/// </summary>
public class MonthlyPointsConfiguration : IEntityTypeConfiguration<MonthlyPoints>
{
    public void Configure(EntityTypeBuilder<MonthlyPoints> builder)
    {
        // 1. Đặt tên bảng
        builder.ToTable("MonthlyPoints");

        // 2. Thiết lập Composite Primary Key (CustomerId + Month + Year)
        // Đảm bảo mỗi customer chỉ có 1 record cho mỗi tháng
        builder.HasKey(mp => new { mp.CustomerId, mp.Month, mp.Year });

        // 3. Cấu hình thuộc tính CustomerId
        builder.Property(mp => mp.CustomerId)
            .IsRequired()
            .HasComment("ID khách hàng - FK đến bảng Users");

        // 4. Cấu hình thuộc tính Month
        builder.Property(mp => mp.Month)
            .IsRequired()
            .HasComment("Tháng (1-12)");

        // 5. Cấu hình thuộc tính Year
        builder.Property(mp => mp.Year)
            .IsRequired()
            .HasComment("Năm (VD: 2024)");

        // 6. Cấu hình thuộc tính MonthlyTotal
        builder.Property(mp => mp.MonthlyTotal)
            .IsRequired()
            .HasDefaultValue(0)
            .HasComment("Tổng điểm tích lũy trong tháng");

        // 7. Quan hệ N-1 với User (Customer)
        // Nhiều MonthlyPoints thuộc về một User
        builder.HasOne(mp => mp.Customer)
            .WithMany() // User không cần navigation property tới MonthlyPoints
            .HasForeignKey(mp => mp.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
