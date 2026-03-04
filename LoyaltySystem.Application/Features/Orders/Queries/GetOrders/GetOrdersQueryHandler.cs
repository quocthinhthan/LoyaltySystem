using LoyaltySystem.Application.Common.Interfaces;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Interfaces;
using MediatR;
using System.Linq;

namespace LoyaltySystem.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, GetOrdersResult>
{
    private readonly IGenericRepository<Order> _orderRepository;
    private readonly IGenericRepository<User> _userRepository;
    private readonly ICurrentUserService _currentUserService; // Inject Context

    public GetOrdersQueryHandler(
        IGenericRepository<Order> orderRepository,
        IGenericRepository<User> userRepository,
        ICurrentUserService currentUserService)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetOrdersResult> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        // 1. Lấy thông tin từ Identity Context
        var currentUserRole = _currentUserService.Role;
        var currentUserId = int.Parse(_currentUserService.UserId ?? "0");

        // 2. Khởi tạo Queryable
        var orders = _orderRepository.Query();
        var users = _userRepository.Query();

        // 3. Phân quyền dữ liệu (Data Isolation)
        if (currentUserRole == "Customer")
        {
            // Nếu là khách hàng, CHỈ được thấy đơn hàng của chính mình
            orders = orders.Where(o => o.CustomerId == currentUserId);
        }

        // 4. Lọc theo thời gian
        if (request.StartDate.HasValue)
        {
            orders = orders.Where(o => o.TimeCreate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            var endDate = request.EndDate.Value.AddDays(1);
            orders = orders.Where(o => o.TimeCreate < endDate);
        }

        // 5. Lọc theo số điện thoại (Chỉ áp dụng cho Staff/Admin)
        if (!string.IsNullOrEmpty(request.CustomerPhone) && currentUserRole != "Customer")
        {
            orders = orders.Where(o =>
                users.Any(u => u.UserId == o.CustomerId && u.PhoneNumber.Contains(request.CustomerPhone)));
        }

        // 6. Sắp xếp
        string sortBy = (request.SortBy ?? "TimeCreate").ToLower();
        if (sortBy == "price")
        {
            orders = request.IsDescending ? orders.OrderByDescending(o => o.Price) : orders.OrderBy(o => o.Price);
        }
        else
        {
            orders = request.IsDescending ? orders.OrderByDescending(o => o.TimeCreate) : orders.OrderBy(o => o.TimeCreate);
        }

        // 7. Phân trang
        var totalRecords = orders.Count();
        var pagedOrders = orders
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // 8. Lấy thông tin User liên quan (Batching)
        var userIds = pagedOrders.Select(o => o.CustomerId).Concat(pagedOrders.Select(o => o.StaffId)).Distinct().ToList();
        var relatedUsers = users.Where(u => userIds.Contains(u.UserId)).ToDictionary(u => u.UserId);

        // 9. Map DTO
        var orderDtos = pagedOrders.Select(o => {
            relatedUsers.TryGetValue(o.CustomerId, out var customer);
            relatedUsers.TryGetValue(o.StaffId, out var staff);

            return new OrderDto(
                o.OrderId,
                customer?.UserName ?? "Unknown",
                customer?.PhoneNumber ?? "N/A",
                o.Price,
                (int)(o.Price / 1000),
                o.TimeCreate,
                staff?.UserName ?? "Unknown"
            );
        }).ToList();

        // 10. Metadata phân trang
        int totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);
        return new GetOrdersResult(orderDtos, new PaginationMetadata(request.PageNumber, request.PageSize, totalRecords, totalPages));
    }
}