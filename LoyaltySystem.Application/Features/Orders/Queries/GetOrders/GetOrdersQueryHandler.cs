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
        // ===== 1. Kiểm tra quyền =====
        if (request.Role == "Customer")
        {
            if (request.UserQueryId != request.UserId.ToString())
            {
                throw new UnauthorizedAccessException("Người dùng không thể xem đơn hàng của người khác.");
            }
        }

        // ===== 2. Bắt đầu query từ database (KHÔNG load hết vào RAM) =====
        var orders = _unitOfWork.Orders.Query();
        var users = _unitOfWork.Users.Query();

        // ===== 3. Lọc theo UserId =====
        if (request.UserId > 0)
        {
            query = query.Where(o => o.CustomerId == request.UserId);
        }

        // ===== 4. Lọc theo thời gian =====
        if (request.StartDate.HasValue)
        {
            DateTime startDate = request.StartDate.Value;
            orders = orders.Where(o => o.TimeCreate >= startDate);
        }

        if (request.EndDate.HasValue)
        {
            DateTime endDate = request.EndDate.Value.AddDays(1);
            orders = orders.Where(o => o.TimeCreate < endDate);
        }

        // ===== 5. Lọc theo số điện thoại (chỉ Staff/Admin) =====
        if (!string.IsNullOrEmpty(request.CustomerPhone) && request.Role != "Customer")
        {
            string phone = request.CustomerPhone;

            orders = orders.Where(o =>
                users.Any(u => u.UserId == o.CustomerId &&
                               u.PhoneNumber.Contains(phone)));
        }

        // ===== 6. Sorting (viết dễ hiểu bằng if/else) =====
        string sortBy = request.SortBy ?? "timecreate";
        sortBy = sortBy.ToLower();

        if (sortBy == "price")
        {
            if (request.IsDescending)
            {
                orders = orders.OrderByDescending(o => o.Price);
            }
            else
            {
                orders = orders.OrderBy(o => o.Price);
            }
        }
        else
        {
            if (request.IsDescending)
            {
                orders = orders.OrderByDescending(o => o.TimeCreate);
            }
            else
            {
                orders = orders.OrderBy(o => o.TimeCreate);
            }
        }

        // ===== 7. Đếm tổng số record =====
        var totalRecords = orders.Count();

        // ===== 8. Phân trang =====
        int pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        int pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var pagedOrders = orders
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // ===== 9. Join user để map DTO (chỉ lấy user cần thiết) =====
        var userIds = pagedOrders
            .Select(o => o.CustomerId)
            .Concat(pagedOrders.Select(o => o.StaffId))
            .Distinct()
            .ToList();

        var relatedUsersList = users
            .Where(u => userIds.Contains(u.UserId))
            .ToList();

        var relatedUsers = relatedUsersList
            .ToDictionary(u => u.UserId);

        var orderDtos = new List<OrderDto>();

        foreach (var o in pagedOrders)
        {
            string customerName = "Unknown";
            string customerPhone = "N/A";
            string staffName = "Unknown";

            if (relatedUsers.TryGetValue(o.CustomerId, out var customer))
            {
                customerName = customer.UserName;
                customerPhone = customer.PhoneNumber;
            }

            if (relatedUsers.ContainsKey(o.StaffId))
            {
                staffName = relatedUsers[o.StaffId].UserName;
            }

            var dto = new OrderDto(
                o.OrderId,
                customerName,
                customerPhone,
                o.Price,
                (int)(o.Price / 1000),
                o.TimeCreate,
                staffName
            );

            orderDtos.Add(dto);
        }

        // ===== 10. Tính tổng số trang =====
        int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

        var pagination = new PaginationMetadata(
            pageNumber,
            pageSize,
            totalRecords,
            totalPages
        );

        return new GetOrdersResult(orderDtos, pagination);
    }
}