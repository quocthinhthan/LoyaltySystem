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
        // 1. Lấy order
        var order = await _orderRepository.GetByIdAsync(request.OrderId);

        if (order == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy đơn hàng #{request.OrderId}");
        }

        // 2. Kiểm tra quyền
        if (request.Role == "Customer")
        {
            if (!int.TryParse(request.UserId, out int userId) || order.CustomerId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xem đơn hàng này");
            }
        }

        // 3. Lấy customer
        var customer = await _userRepository.GetByIdAsync(order.CustomerId);

        if (customer == null)
        {
            throw new Exception("Không tìm thấy thông tin khách hàng");
        }

        // 4. Lấy staff
        var staff = await _userRepository.GetByIdAsync(order.StaffId);


        // 5. Map DTO
        var result = new OrderDetailResult(
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
            PointsEarned: (int)(order.Price / 1000),
            TimeCreate: order.TimeCreate
        );

        return result;
    }
}