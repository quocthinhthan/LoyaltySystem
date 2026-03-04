namespace LoyaltySystem.Application.Common.Interfaces;

public interface ICurrentUserService
{
    // Lấy ID của người dùng (từ Claim NameIdentifier trong Token)
    string? UserId { get; }

    // Lấy Role của người dùng (từ Claim Role trong Token)
    string? Role { get; }

}