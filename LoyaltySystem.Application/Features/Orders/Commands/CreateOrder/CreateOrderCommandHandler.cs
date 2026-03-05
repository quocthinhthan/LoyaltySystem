using LoyaltySystem.Application.Common.Interfaces;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Interfaces;
using MediatR;

namespace LoyaltySystem.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateOrderCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Bắt đầu Transaction
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var staffIdStr = _currentUserService.UserId;
            if (string.IsNullOrEmpty(staffIdStr))
            {
                throw new UnauthorizedAccessException("Không xác định được nhân viên thực hiện đơn hàng.");
            }

            int staffId = int.Parse(staffIdStr);

            // 2. Kiểm tra khách hàng
            var customer = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.PhoneNumber == request.CustomerPhoneNumber && u.Role == "Customer");

            if (customer == null)
            {
                throw new Exception($"Không tìm thấy khách hàng với số điện thoại {request.CustomerPhoneNumber}");
            }

            // 3. Tính điểm tích lũy
            int pointsEarned = (int)(request.Price / 1000);

            // 4. Tạo Order mới
            var order = new Order
            {
                CustomerId = customer.UserId,
                StaffId = staffId,
                Price = request.Price,
                TimeCreate = DateTime.Now
            };
            _unitOfWork.Orders.Add(order);

            // 5. Cập nhật TotalPoint của khách hàng
            customer.TotalPoint += pointsEarned;
            _unitOfWork.Users.Update(customer);

            // 6. Cập nhật MonthlyPoints
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var monthlyPoint = await _unitOfWork.MonthlyPoints
                .FirstOrDefaultAsync(mp => mp.CustomerId == customer.UserId
                                   && mp.Month == currentMonth
                                   && mp.Year == currentYear);

            if (monthlyPoint == null)
            {
                monthlyPoint = new MonthlyPoints
                {
                    CustomerId = customer.UserId,
                    Month = currentMonth,
                    Year = currentYear,
                    MonthlyTotal = pointsEarned
                };
                _unitOfWork.MonthlyPoints.Add(monthlyPoint);
            }
            else
            {
                monthlyPoint.MonthlyTotal += pointsEarned;
                _unitOfWork.MonthlyPoints.Update(monthlyPoint);
            }

            // 7. Lưu tất cả thay đổi vào Database
            await _unitOfWork.CompleteAsync();

            // 8. Chốt giao dịch (Commit)
            await _unitOfWork.CommitAsync();

            return new CreateOrderResult(
                OrderId: order.OrderId,
                CustomerName: customer.UserName,
                Price: request.Price,
                PointsEarned: pointsEarned,
                TotalPoints: customer.TotalPoint,
                Message: $"Tạo đơn hàng thành công! Khách hàng được cộng {pointsEarned} điểm."
            );
        }
        catch (Exception ex)
        {
            // 9. Nếu có bất kỳ lỗi nào, hoàn tác toàn bộ (Rollback)
            await _unitOfWork.RollbackAsync();

            // Log lỗi hoặc ném tiếp để tầng trên xử lý
            throw new Exception($"Lỗi khi tạo đơn hàng: {ex.Message}", ex);
        }
    }
}