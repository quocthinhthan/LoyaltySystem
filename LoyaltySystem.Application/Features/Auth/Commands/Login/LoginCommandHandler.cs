using LoyaltySystem.Application.Common.Interfaces;
using LoyaltySystem.Domain.Interfaces;
using MediatR;

namespace LoyaltySystem.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(IUnitOfWork unitOfWork, IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Tìm Account theo PhoneNumber
        var account = await _unitOfWork.Account
            .FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber);

        if (account == null || account.Password != request.Password)
        {
            throw new Exception("Số điện thoại hoặc mật khẩu không chính xác."); 
        }

        // 2. Lấy thông tin User
        var user = await _unitOfWork.Users
            .FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber);

        if (user == null)
        {
            throw new Exception("Không tìm thấy hồ sơ người dùng.");
        }

        // 3. Sinh Token
        var token = _jwtTokenGenerator.GenerateToken(user);

        // 4. Trả kết quả
        var result = new AuthResult(
            Token: token,
            UserId: user.UserId,
            UserName: user.UserName,
            Role: user.Role
        );

        return result;
    }
}