using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoyaltySystem.Application.Features.Auth.Commands
{
    public class RegisterHandler : IRequestHandler<RegisterCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public RegisterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var existingAccount = await _unitOfWork.Account.GetByIdAsync(request.PhoneNumber);

            if (existingAccount != null)
            {
                // Bạn có thể quăng một Exception hoặc trả về thông báo lỗi tại đây
                throw new Exception("Số điện thoại này đã được đăng ký!");
            }

            // 2. Tạo thực thể User (Thông tin hồ sơ)
            var newUser = new User
            {
                UserName = request.UserName,
                PhoneNumber = request.PhoneNumber,
                Role = request.Role, // "Customer", "Staff", hoặc "Admin"
                TotalPoint = 0
            };
            _unitOfWork.Users.Add(newUser);

            // Lưu tạm để lấy UserId tự sinh từ SQL Server
            await _unitOfWork.CompleteAsync();

            // 3. Tạo thực thể Account (Thông tin đăng nhập)
            var newAccount = new Account
            {
                PhoneNumber = request.PhoneNumber,
                Password = request.Password, // Ở mức thực tập, bạn nên tìm hiểu thêm về BCrypt để hash mật khẩu
            };
            _unitOfWork.Account.Add(newAccount);

            // 4. Chốt giao dịch cuối cùng
            await _unitOfWork.CompleteAsync();

            return newUser.UserId;
        }
    }
}
