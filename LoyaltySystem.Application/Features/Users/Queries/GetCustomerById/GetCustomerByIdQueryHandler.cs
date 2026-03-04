using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Interfaces;
using MediatR;

namespace LoyaltySystem.Application.Features.Users.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDetailResult>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Order> _orderRepository;
    private readonly IGenericRepository<MonthlyPoints> _monthlyPointsRepository;

    public GetCustomerByIdQueryHandler(
        IGenericRepository<User> userRepository,
        IGenericRepository<Order> orderRepository,
        IGenericRepository<MonthlyPoints> monthlyPointsRepository)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _monthlyPointsRepository = monthlyPointsRepository;
    }

    public async Task<CustomerDetailResult> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra quyền truy cập (Giống logic GetOrders)
        if (request.CurrentUserRole == "Customer" && request.CurrentUserId != request.CustomerId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xem thông tin của người khác.");
        }

        // 2. Lấy thông tin Customer
        var customer = await _userRepository.FirstOrDefaultAsync(u => u.UserId == request.CustomerId);

        if (customer == null || customer.Role != "Customer")
        {
            throw new KeyNotFoundException($"Không tìm thấy khách hàng #{request.CustomerId}");
        }

        // 2. Thống kê đơn hàng thông qua IQueryable (System.Linq)
        // Lưu ý: Không dùng .ToListAsync() của EF mà dùng .ToList() của Linq
        var ordersQuery = _orderRepository.Query()
            .Where(o => o.CustomerId == request.CustomerId);

        var totalOrders = ordersQuery.Count();
        var totalSpent = ordersQuery.Sum(o => o.Price);

        var recentOrders = ordersQuery
            .OrderByDescending(o => o.TimeCreate)
            .Take(10)
            .Select(o => new RecentOrderDto(
                o.OrderId,
                o.Price,
                (int)(o.Price / 1000),
                o.TimeCreate
            ))
            .ToList();

        // 3. Xếp hạng & Tổng số khách hàng
        var rank = _userRepository.Query()
            .Count(u => u.Role == "Customer" && u.TotalPoint > customer.TotalPoint) + 1;

        var totalCustomers = _userRepository.Query()
            .Count(u => u.Role == "Customer");

        // 4. Điểm tháng hiện tại
        var now = DateTime.Now;
        var currentMonthPoints = _monthlyPointsRepository.Query()
            .Where(mp => mp.CustomerId == request.CustomerId
                      && mp.Month == now.Month
                      && mp.Year == now.Year)
            .Select(mp => mp.MonthlyTotal)
            .FirstOrDefault();

        return new CustomerDetailResult(
            UserId: customer.UserId,
            UserName: customer.UserName,
            PhoneNumber: customer.PhoneNumber,
            TotalPoint: customer.TotalPoint,
            Rank: rank,
            TotalCustomers: totalCustomers,
            TotalOrders: totalOrders,
            TotalSpent: totalSpent,
            CurrentMonthPoints: currentMonthPoints,
            RecentOrders: recentOrders
        );
    }
}