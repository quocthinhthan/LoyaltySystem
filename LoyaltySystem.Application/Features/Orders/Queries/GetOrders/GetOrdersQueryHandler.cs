using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Interfaces;
using MediatR;
using System.Linq;

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
        // 1. Kiểm tra quyền (Authorization - Validator đã đảm bảo UserQueryId là số)
        if (request.Role == "Customer")
        {
            if (request.UserQueryId != request.UserId.ToString())
            {
                throw new UnauthorizedAccessException("Người dùng không thể xem đơn hàng của người khác.");
            }
        }

        // 2. Query database (Sử dụng IQueryable để tối ưu việc lọc tại DB)
        var orders = _orderRepository.Query();
        var users = _userRepository.Query();

        // 3. Lọc theo UserId (Lấy đơn hàng của một User cụ thể)
        if (request.UserId > 0)
        {
            orders = orders.Where(o => o.CustomerId == request.UserId);
        }

        // 4. Filter theo thời gian
        if (request.StartDate.HasValue)
        {
            orders = orders.Where(o => o.TimeCreate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            // Kết thúc ngày là 23:59:59 của ngày đó
            var endDate = request.EndDate.Value.AddDays(1);
            orders = orders.Where(o => o.TimeCreate < endDate);
        }

        // 5. Filter theo phone (Chỉ Staff/Admin mới được dùng lọc theo Phone)
        if (!string.IsNullOrEmpty(request.CustomerPhone) && request.Role != "Customer")
        {
            orders = orders.Where(o =>
                users.Any(u => u.UserId == o.CustomerId && u.PhoneNumber.Contains(request.CustomerPhone)));
        }

        // 6. Sorting (Validator đã lọc các field hợp lệ)
        string sortBy = (request.SortBy ?? "TimeCreate").ToLower();

        if (sortBy == "price")
        {
            orders = request.IsDescending ? orders.OrderByDescending(o => o.Price) : orders.OrderBy(o => o.Price);
        }
        else
        {
            orders = request.IsDescending ? orders.OrderByDescending(o => o.TimeCreate) : orders.OrderBy(o => o.TimeCreate);
        }

        // 7. Thực hiện phân trang (Mặc định PageNumber và PageSize đã hợp lệ nhờ Validator)
        var totalRecords = orders.Count();
        var pagedOrders = orders
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // 8. Lấy thông tin User liên quan để map tên/SĐT
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
                (int)(o.Price / 1000), // Quy tắc tích điểm
                o.TimeCreate,
                staff?.UserName ?? "Unknown"
            );
        }).ToList();

        // 10. Trả về kết quả phân trang
        int totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);
        return new GetOrdersResult(orderDtos, new PaginationMetadata(request.PageNumber, request.PageSize, totalRecords, totalPages));
    }
}