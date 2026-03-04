using LoyaltySystem.Application.Common.Interfaces;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Interfaces;
using MediatR;

namespace LoyaltySystem.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailResult>
{
    private readonly IGenericRepository<Order> _orderRepository;
    private readonly IGenericRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService; // Dịch vụ lấy User Context

    public GetOrderByIdQueryHandler(
        IGenericRepository<Order> orderRepository,
        IGenericRepository<User> userRepository,
        ICurrentUserService currentUserService)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<OrderDetailResult> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Lấy thông tin đơn hàng
        var order = await _orderRepository.GetByIdAsync(request.OrderId);

        if (order == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{request.OrderId}");
        }

        // 2. Kiểm tra quyền sở hữu dựa trên Current User Context
        var currentUserRole = _currentUserService.Role;
        var currentUserId = _currentUserService.UserId;

        if (currentUserRole == "Customer")
        {
            // So sánh CustomerId của đơn hàng với ID người dùng từ Token
            if (order.CustomerId.ToString() != currentUserId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xem đơn hàng của người khác.");
            }
        }

        // 3. Lấy thông tin Customer liên quan
        var customer = await _userRepository.GetByIdAsync(order.CustomerId);
        if (customer == null)
        {
            throw new Exception("Dữ liệu khách hàng không nhất quán trong hệ thống.");
        }

        // 4. Lấy thông tin Staff thực hiện đơn hàng (nếu có)
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
            PointsEarned: (int)(order.Price / 1000), // Logic tích điểm: 1000đ = 1 điểm
            TimeCreate: order.TimeCreate
        );
    }
}