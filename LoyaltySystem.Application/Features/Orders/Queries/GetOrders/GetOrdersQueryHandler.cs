using LoyaltySystem.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        // 1. Khởi tạo query từ IQueryable (Chưa thực thi SQL)
        var query = _unitOfWork.Orders.GetQueryable();

        // 2. Kiểm tra quyền truy cập
        if (request.Role == "Customer" && request.UserQueryId != request.UserId.ToString())
        {
            throw new UnauthorizedAccessException("Người dùng không thể xem các đơn hàng của người dùng khác!");
        }

        // 3. Filter theo UserId
        if (request.UserId > 0)
        {
            query = query.Where(o => o.CustomerId == request.UserId);
        }

        // 4. Filter theo thời gian
        if (request.StartDate.HasValue)
            query = query.Where(o => o.TimeCreate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
        {
            var endDateInclusive = request.EndDate.Value.AddDays(1);
            query = query.Where(o => o.TimeCreate < endDateInclusive);
        }

        // 5. Filter theo số điện thoại (Dùng JOIN chuẩn của LINQ)
        if (!string.IsNullOrEmpty(request.CustomerPhone) && request.Role != "Customer")
        {
            var usersQuery = _unitOfWork.Users.GetQueryable();
            query = from o in query
                    join u in usersQuery on o.CustomerId equals u.UserId
                    where u.PhoneNumber.Contains(request.CustomerPhone)
                    select o;
        }

        // 6. Sorting
        query = request.SortBy.ToLower() switch
        {
            "price" => request.IsDescending ? query.OrderByDescending(o => o.Price) : query.OrderBy(o => o.Price),
            _ => request.IsDescending ? query.OrderByDescending(o => o.TimeCreate) : query.OrderBy(o => o.TimeCreate)
        };

        // 7. Đếm tổng số record trên Database
        var totalRecords = await query.CountAsync(cancellationToken);

        // 8. Phân trang và Lấy dữ liệu kèm thông tin User (Tối ưu nhất)
        // Sử dụng Join ở đây giúp SQL lấy tên khách hàng và nhân viên trong 1 lần quét
        var userQueryable = _unitOfWork.Users.GetQueryable();

        var pagedOrders = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new
            {
                Order = o,
                Customer = userQueryable.FirstOrDefault(u => u.UserId == o.CustomerId),
                Staff = userQueryable.FirstOrDefault(u => u.UserId == o.StaffId)
            })
            .ToListAsync(cancellationToken);

        // 9. Map kết quả sang DTO
        var orderDtos = pagedOrders.Select(x => new OrderDto(
            OrderId: x.Order.OrderId,
            CustomerName: x.Customer?.UserName ?? "Unknown",
            CustomerPhone: x.Customer?.PhoneNumber ?? "N/A",
            Price: x.Order.Price,
            PointsEarned: (int)(x.Order.Price / 1000),
            TimeCreate: x.Order.TimeCreate,
            StaffName: x.Staff?.UserName ?? "Unknown"
        )).ToList();

        // 10. Metadata phân trang
        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);
        var pagination = new PaginationMetadata(request.PageNumber, request.PageSize, totalRecords, totalPages);

        return new GetOrdersResult(orderDtos, pagination);
    }
}