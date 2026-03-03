namespace LoyaltySystem.Application.Features.Auth.Commands.Login;

public record AuthResult(
    string Token,
    int UserId,
    string UserName,
    string Role
);