using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Interfaces;
using MediatR;
using BCrypt.Net;

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
            await _unitOfWork.BeginTransactionAsync();
            try
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
                    Password = BCrypt.Net.BCrypt.HashPassword(request.Password), //  Băm password trước khi lưu
                };
                _unitOfWork.Account.Add(newAccount);

                // 4. Chốt giao dịch cuối cùng
                await _unitOfWork.CommitAsync();

                return newUser.UserId;
            }
            catch (Exception ex)
            {
                // Nếu có bất kỳ lỗi nào ở trên, Rollback toàn bộ
                // User đã Add ở bước 2 cũng sẽ bị xóa khỏi DB
                await _unitOfWork.RollbackAsync();

                // Log lỗi hoặc ném tiếp exception tùy logic của bạn
                throw new Exception($"Đăng ký thất bại: {ex.Message}");
            }
        }
    }
}
