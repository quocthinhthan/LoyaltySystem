using LoyaltySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LoyaltySystem.Infrastructure.Data.Configurations;

/// <summary>
/// Cấu hình cho bảng Account (Tài khoản đăng nhập)
/// Quan hệ: 1-1 với User
/// </summary>
public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        // 1. Đặt tên bảng
        builder.ToTable("Accounts");

        // 2. Thiết lập Primary Key
        builder.HasKey(a => a.PhoneNumber);

        // 3. Cấu hình thuộc tính PhoneNumber
        builder.Property(a => a.PhoneNumber)
            .IsRequired()
            .HasMaxLength(15)
            .HasComment("Số điện thoại - là Primary Key");

        // 4. Cấu hình thuộc tính Password
        builder.Property(a => a.Password)
            .IsRequired()
            .HasMaxLength(255)
            .HasComment("Mật khẩu đã hash");

        // 5. Quan hệ 1-1 với User
        // Một Account có một User, một User có một Account
        builder.HasOne(a => a.User)
            .WithOne(u => u.Account)
            .HasForeignKey<Account>(a => a.PhoneNumber)
            .HasPrincipalKey<User>(u => u.PhoneNumber)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
