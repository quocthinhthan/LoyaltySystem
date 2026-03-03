using LoyaltySystem.Domain.Interfaces;
using MediatR;

namespace LoyaltySystem.Application.Features.Orders.Queries.GetOrdersByCustomer;

public class GetOrdersByCustomerQueryHandler 
    : IRequestHandler<GetOrdersByCustomerQuery, GetOrdersByCustomerResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOrdersByCustomerQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetOrdersByCustomerResult> Handle(
        GetOrdersByCustomerQuery request, 
        CancellationToken cancellationToken)
    {
        // 1. Kiểm tra quyền (chỉ Staff/Admin)
        if (request.CallerRole != "Staff" && request.CallerRole != "Admin")
        {
            throw new UnauthorizedAccessException("Chỉ nhân viên mới có quyền xem");
        }

        // 2. Lấy thông tin customer
        var customer = await _unitOfWork.Users.GetByIdAsync(request.CustomerId);
        if (customer == null)
        {
            throw new Exception($"Không tìm thấy khách hàng #{request.CustomerId}");
        }

        // 3. Lấy danh sách orders và apply filters
        var orders = (await _unitOfWork.Orders.GetAllAsync())
            .Where(o => o.CustomerId == request.CustomerId)
            .AsQueryable();

        // 4. ✅ Filter theo thời gian
        if (request.StartDate.HasValue)
        {
            orders = orders.Where(o => o.TimeCreate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            var endDateInclusive = request.EndDate.Value.AddDays(1);
            orders = orders.Where(o => o.TimeCreate < endDateInclusive);
        }

        // 5. Sắp xếp
        orders = orders.OrderByDescending(o => o.TimeCreate);

        // 6. ✅ Tính thống kê (trước khi phân trang)
        var totalRecords = orders.Count();
        var totalSpent = orders.Sum(o => o.Price);

        // 7. ✅ Phân trang
        var pagedOrders = orders
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // 8. Map sang DTO
        var orderDtos = pagedOrders.Select(o => new OrderSummaryDto(
            OrderId: o.OrderId,
            Price: o.Price,
            PointsEarned: (int)(o.Price / 1000),
            TimeCreate: o.TimeCreate
        )).ToList();

        // 9. ✅ Tính metadata
        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        return new GetOrdersByCustomerResult(
            Customer: new CustomerSummary(
                UserId: customer.UserId,
                UserName: customer.UserName,
                PhoneNumber: customer.PhoneNumber,
                TotalPoints: customer.TotalPoint,
                TotalOrders: totalRecords,
                TotalSpent: totalSpent
            ),
            Orders: orderDtos,
            Pagination: new PaginationInfo(
                CurrentPage: request.PageNumber,
                PageSize: request.PageSize,
                TotalRecords: totalRecords,
                TotalPages: totalPages
            )
        );
    }
}