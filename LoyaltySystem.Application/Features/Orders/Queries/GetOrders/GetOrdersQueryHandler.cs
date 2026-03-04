using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Interfaces;
using MediatR;

namespace LoyaltySystem.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, GetOrdersResult>
{
    private readonly IGenericRepository<Order> _orderRepository;
    private readonly IGenericRepository<User> _userRepository;

    public GetOrdersQueryHandler(
        IGenericRepository<Order> orderRepository,
        IGenericRepository<User> userRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
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

        // ===== 2. Query database =====
        var orders = _orderRepository.Query();
        var users = _userRepository.Query();

        // ===== 3. Lọc theo UserId =====
        if (request.UserId > 0)
        {
            orders = orders.Where(o => o.CustomerId == request.UserId);
        }

        // ===== 4. Filter theo thời gian =====
        if (request.StartDate.HasValue)
        {
            var startDate = request.StartDate.Value;
            orders = orders.Where(o => o.TimeCreate >= startDate);
        }

        if (request.EndDate.HasValue)
        {
            var endDate = request.EndDate.Value.AddDays(1);
            orders = orders.Where(o => o.TimeCreate < endDate);
        }

        // ===== 5. Filter theo phone =====
        if (!string.IsNullOrEmpty(request.CustomerPhone) && request.Role != "Customer")
        {
            string phone = request.CustomerPhone;

            orders = orders.Where(o =>
                users.Any(u => u.UserId == o.CustomerId &&
                               u.PhoneNumber.Contains(phone)));
        }

        // ===== 6. Sorting =====
        string sortBy = request.SortBy ?? "timecreate";
        sortBy = sortBy.ToLower();

        if (sortBy == "price")
        {
            orders = request.IsDescending
                ? orders.OrderByDescending(o => o.Price)
                : orders.OrderBy(o => o.Price);
        }
        else
        {
            orders = request.IsDescending
                ? orders.OrderByDescending(o => o.TimeCreate)
                : orders.OrderBy(o => o.TimeCreate);
        }

        // ===== 7. Count =====
        var totalRecords = orders.Count();

        // ===== 8. Pagination =====
        int pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        int pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var pagedOrders = orders
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // ===== 9. Lấy user liên quan =====
        var userIds = pagedOrders
            .Select(o => o.CustomerId)
            .Concat(pagedOrders.Select(o => o.StaffId))
            .Distinct()
            .ToList();

        var relatedUsersList = users
            .Where(u => userIds.Contains(u.UserId))
            .ToList();

        var relatedUsers = relatedUsersList.ToDictionary(u => u.UserId);

        // ===== 10. Map DTO =====
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

            if (relatedUsers.TryGetValue(o.StaffId, out var staff))
            {
                staffName = staff.UserName;
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

        // ===== 11. Pagination metadata =====
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