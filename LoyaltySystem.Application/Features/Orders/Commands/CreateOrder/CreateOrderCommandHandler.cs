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
        var staffIdStr = _currentUserService.UserId;

        if (string.IsNullOrEmpty(staffIdStr))
        {
            throw new UnauthorizedAccessException("Không xác định được nhân viên thực hiện đơn hàng.");
        }

        int staffId = int.Parse(staffIdStr);

        // 2. Kiểm tra khách hàng có tồn tại không
        var customer = await _unitOfWork.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.CustomerPhoneNumber && u.Role == "Customer");

        if (customer == null)
        {
            throw new Exception($"Không tìm thấy khách hàng với số điện thoại {request.CustomerPhoneNumber}");
        }

        // 3. Tính điểm tích lũy (1000đ = 1 điểm)
        int pointsEarned = (int)(request.Price / 1000);

        // 4. Tạo Order mới
        var order = new Order
        {
            CustomerId = customer.UserId,
            StaffId = staffId, // Dùng staffId đã parse
            Price = request.Price,
            TimeCreate = DateTime.Now
        };

        _unitOfWork.Orders.Add(order);

        // 4. Cập nhật TotalPoint của khách hàng
        customer.TotalPoint += pointsEarned;
        _unitOfWork.Users.Update(customer);

        // 5. Cập nhật MonthlyPoints
        var currentMonth = DateTime.Now.Month;
        var currentYear = DateTime.Now.Year;

        var monthlyPoint = await _unitOfWork.MonthlyPoints
            .FirstOrDefaultAsync(mp => mp.CustomerId == customer.UserId 
                               && mp.Month == currentMonth 
                               && mp.Year == currentYear);

        if (monthlyPoint == null)
        {
            // Tạo mới nếu chưa có
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
            // Cộng dồn nếu đã có
            monthlyPoint.MonthlyTotal += pointsEarned;
            _unitOfWork.MonthlyPoints.Update(monthlyPoint);
        }

        // 6. Lưu tất cả thay đổi
        await _unitOfWork.CompleteAsync();

        // 7. Trả kết quả
        return new CreateOrderResult(
            OrderId: order.OrderId,
            CustomerName: customer.UserName,
            Price: request.Price,
            PointsEarned: pointsEarned,
            TotalPoints: customer.TotalPoint,
            Message: $"Tạo đơn hàng thành công! Khách hàng được cộng {pointsEarned} điểm."
        );
    }
}