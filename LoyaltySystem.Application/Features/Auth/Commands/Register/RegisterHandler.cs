using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Interfaces;
using MediatR;
using BCrypt.Net; // Thêm thư viện BCrypt.Net-Next

namespace LoyaltySystem.Application.Features.Auth.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;

    public RegisterHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra sự tồn tại (Business Validation)
        var existingAccount = await _unitOfWork.Account
            .FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber);

        if (existingAccount != null)
        {
            throw new Exception("Số điện thoại này đã được đăng ký!");
        }

        // 2. Mã hóa mật khẩu (Security)
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 3. Tạo thực thể User
        var newUser = new User
        {
            UserName = request.UserName,
            PhoneNumber = request.PhoneNumber,
            Role = request.Role,
            TotalPoint = 0
        };

        // 4. Tạo thực thể Account gắn liền với User
        var newAccount = new Account
        {
            PhoneNumber = request.PhoneNumber,
            Password = passwordHash // Lưu hash thay vì password thô
        };

        // 5. Lưu tất cả vào Database trong một Transaction duy nhất
        _unitOfWork.Users.Add(newUser);
        _unitOfWork.Account.Add(newAccount);

        // Chỉ gọi CompleteAsync MỘT LẦN duy nhất để đảm bảo Atomicity (Tất cả hoặc không gì cả)
        await _unitOfWork.CompleteAsync();

        return newUser.UserId;
    }
}