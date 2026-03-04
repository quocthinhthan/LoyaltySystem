using LoyaltySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        var userQuery = _unitOfWork.Users.GetQueryable();
        var orderQuery = _unitOfWork.Orders.GetQueryable();

        // 1. Thực hiện Join tất cả thông tin trong 1 câu Query duy nhất
        var result = await (from o in orderQuery
                            where o.OrderId == request.OrderId
                            select new
                            {
                                Order = o,
                                // Join lấy thông tin Customer
                                Customer = userQuery.FirstOrDefault(u => u.UserId == o.CustomerId),
                                // Join lấy thông tin Staff
                                Staff = userQuery.FirstOrDefault(u => u.UserId == o.StaffId)
                            }).FirstOrDefaultAsync(cancellationToken);

        // 2. Kiểm tra tồn tại
        if (result == null || result.Order == null)
        {
            throw new Exception($"Không tìm thấy đơn hàng #{request.OrderId}");
        }

        // 3. Kiểm tra quyền (Sử dụng dữ liệu đã lấy từ Query)
        if (request.Role == "Customer" && result.Order.CustomerId != int.Parse(request.UserId))
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xem đơn hàng này");
        }

        if (result.Customer == null)
        {
            throw new Exception("Không tìm thấy thông tin khách hàng");
        }

        // 4. Map sang DTO (Dữ liệu đã có sẵn trong biến result, không cần await thêm)
        return new OrderDetailResult(
            OrderId: result.Order.OrderId,
            Customer: new CustomerInfo(
                UserId: result.Customer.UserId,
                UserName: result.Customer.UserName,
                PhoneNumber: result.Customer.PhoneNumber,
                TotalPoints: result.Customer.TotalPoint
            ),
            Staff: new StaffInfo(
                UserId: result.Staff?.UserId ?? 0,
                UserName: result.Staff?.UserName ?? "Unknown"
            ),
            Price: result.Order.Price,
            PointsEarned: (int)(result.Order.Price / 1000),
            TimeCreate: result.Order.TimeCreate
        );
    }
}
