using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Interfaces;
using MediatR;

namespace LoyaltySystem.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailResult>
{
    private readonly IGenericRepository<Order> _orderRepository;
    private readonly IGenericRepository<User> _userRepository;

    public GetOrderByIdQueryHandler(
        IGenericRepository<Order> orderRepository,
        IGenericRepository<User> userRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
    }

    public async Task<OrderDetailResult> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Lấy thông tin đơn hàng
        var order = await _orderRepository.GetByIdAsync(request.OrderId);

        if (order == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{request.OrderId}");
        }

        // 2. Kiểm tra quyền sở hữu (Business Validation)
        // Validator đã đảm bảo UserId là số, nên ta có thể Parse an toàn tại đây
        if (request.Role == "Customer")
        {
            int currentUserId = int.Parse(request.UserId);
            if (order.CustomerId != currentUserId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xem đơn hàng của người khác.");
            }
        }

        // 3. Lấy thông tin Customer
        var customer = await _userRepository.GetByIdAsync(order.CustomerId);
        if (customer == null)
        {
            throw new Exception("Dữ liệu khách hàng không nhất quán.");
        }

        // 4. Lấy thông tin Staff (nếu có)
        var staff = await _userRepository.GetByIdAsync(order.StaffId);

        // 5. Map sang DTO kết quả
        return new OrderDetailResult(
            OrderId: order.OrderId,
            Customer: new CustomerInfo(
                UserId: customer.UserId,
                UserName: customer.UserName,
                PhoneNumber: customer.PhoneNumber,
                TotalPoints: customer.TotalPoint
            ),
            Staff: new StaffInfo(
                UserId: staff?.UserId ?? 0,
                UserName: staff?.UserName ?? "Unknown"
            ),
            Price: order.Price,
            PointsEarned: (int)(order.Price / 1000), // Quy tắc tích điểm
            TimeCreate: order.TimeCreate
        );
    }
}