using MediatR;

namespace LoyaltySystem.Application.Features.Auth.Commands.Login;

// IRequest<AuthResult> báo cho MediatR biết Command này sẽ trả về cục dữ liệu AuthResult
public record LoginCommand(string PhoneNumber, string Password) : IRequest<AuthResult>;