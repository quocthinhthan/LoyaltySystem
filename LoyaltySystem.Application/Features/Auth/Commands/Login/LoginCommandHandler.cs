using LoyaltySystem.Application.Common.Interfaces;
using MediatR;
using System.Linq; // Sử dụng LINQ thuần của C#

namespace LoyaltySystem.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IAppDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(IAppDbContext context, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Tìm Account qua IQueryable (Dùng hàm đồng bộ FirstOrDefault)
        var account = _context.Accounts
            .FirstOrDefault(x => x.PhoneNumber == request.PhoneNumber);

        if (account == null || account.Password != request.Password)
        {
            throw new Exception("Số điện thoại hoặc mật khẩu không chính xác."); 
        }

        // 2. Lấy thông tin User
        var user = _context.Users
            .FirstOrDefault(x => x.PhoneNumber == request.PhoneNumber);

        if (user == null)
        {
            throw new Exception("Không tìm thấy hồ sơ người dùng.");
        }

        // 3. Sinh Token
        var token = _jwtTokenGenerator.GenerateToken(user);

        // 4. Trả kết quả (Bọc trong Task.FromResult vì hàm interface bắt buộc trả về Task)
        var result = new AuthResult(
            Token: token,
            UserId: user.UserId,
            UserName: user.UserName,
            Role: user.Role
        );

        return Task.FromResult(result);
    }
}