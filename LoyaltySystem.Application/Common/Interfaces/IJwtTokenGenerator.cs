using LoyaltySystem.Domain.Entities;

namespace LoyaltySystem.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}