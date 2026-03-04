using LoyaltySystem.Domain.Interfaces;
using MediatR;

namespace LoyaltySystem.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOrderByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderDetailResult> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {

        // 1. Lấy order
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);

        
        if (order == null)
        {
            throw new Exception($"Không tìm thấy đơn hàng #{request.OrderId}");
        }



        // 2. Kiểm tra quyền: Customer chỉ xem order của mình
        if (request.Role == "Customer" && order.CustomerId != int.Parse(request.UserId))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xem đơn hàng này");
        }

        // 3. Lấy thông tin customer
        var customer = await _unitOfWork.Users.GetByIdAsync(order.CustomerId);
        if (customer == null)
        {
            throw new Exception("Không tìm thấy thông tin khách hàng");
        }

        // 4. Lấy thông tin staff
        var staff = await _unitOfWork.Users.GetByIdAsync(order.StaffId);

        // 5. Map sang DTO
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
            PointsEarned: (int)(order.Price / 1000),
            TimeCreate: order.TimeCreate
        );
    }
}