using LoyaltySystem.Domain.Interfaces;
using MediatR;

namespace LoyaltySystem.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, GetOrdersResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOrdersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetOrdersResult> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        // 1. ✅ Lấy users một lần ở đầu để dùng cho nhiều mục đích
        var users = await _unitOfWork.Users.GetAllAsync();
        var userDict = users.ToDictionary(u => u.UserId);

        // 2. Lấy tất cả orders
        var orders = (await _unitOfWork.Orders.GetAllAsync()).AsQueryable();

        // 3. Phân quyền: Customer chỉ xem orders của mình
        if (request.Role == "Customer")
        {
            orders = orders.Where(o => o.CustomerId == request.UserId);
        }

        // 4. ✅ Filter theo thời gian
        if (request.StartDate.HasValue)
        {
            orders = orders.Where(o => o.TimeCreate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            // Thêm 1 ngày để bao gồm cả ngày EndDate
            var endDateInclusive = request.EndDate.Value.AddDays(1);
            orders = orders.Where(o => o.TimeCreate < endDateInclusive);
        }

        // 5. ✅ Filter theo số điện thoại khách hàng (dành cho Staff)
        if (!string.IsNullOrEmpty(request.CustomerPhone) && request.Role != "Customer")
        {
            // ✅ Sử dụng userDict đã lấy ở trên
            var customerIds = userDict.Values
                .Where(u => u.PhoneNumber.Contains(request.CustomerPhone))
                .Select(u => u.UserId)
                .ToList();

            orders = orders.Where(o => customerIds.Contains(o.CustomerId));
        }

        // 6. ✅ Sorting
        orders = request.SortBy.ToLower() switch
        {
            "price" => request.IsDescending
                ? orders.OrderByDescending(o => o.Price)
                : orders.OrderBy(o => o.Price),
            "timecreate" or _ => request.IsDescending
                ? orders.OrderByDescending(o => o.TimeCreate)
                : orders.OrderBy(o => o.TimeCreate)
        };

        // 7. ✅ Đếm tổng số records (trước khi phân trang)
        var totalRecords = orders.Count();

        // 8. ✅ Pagination
        var pagedOrders = orders
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // 9. ✅ Map sang DTO (sử dụng userDict đã có)
        var orderDtos = pagedOrders.Select(o => new OrderDto(
            OrderId: o.OrderId,
            CustomerName: userDict.ContainsKey(o.CustomerId)
                ? userDict[o.CustomerId].UserName
                : "Unknown",
            CustomerPhone: userDict.ContainsKey(o.CustomerId)
                ? userDict[o.CustomerId].PhoneNumber
                : "N/A",
            Price: o.Price,
            PointsEarned: (int)(o.Price / 1000),
            TimeCreate: o.TimeCreate,
            StaffName: userDict.ContainsKey(o.StaffId)
                ? userDict[o.StaffId].UserName
                : "Unknown"
        )).ToList();

        // 10. ✅ Tạo metadata phân trang
        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        var pagination = new PaginationMetadata(
            CurrentPage: request.PageNumber,
            PageSize: request.PageSize,
            TotalRecords: totalRecords,
            TotalPages: totalPages
        );

        return new GetOrdersResult(orderDtos, pagination);
    }
}