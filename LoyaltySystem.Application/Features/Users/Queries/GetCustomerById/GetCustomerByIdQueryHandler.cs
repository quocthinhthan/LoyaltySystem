using LoyaltySystem.Application.Common.Interfaces;
using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Interfaces;
using MediatR;
using System.Linq;

namespace LoyaltySystem.Application.Features.Users.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDetailResult>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Order> _orderRepository;
    private readonly IGenericRepository<MonthlyPoints> _monthlyPointsRepository;
    private readonly ICurrentUserService _currentUserService; // Dịch vụ lấy ngữ cảnh người dùng

    public GetCustomerByIdQueryHandler(
        IGenericRepository<User> userRepository,
        IGenericRepository<Order> orderRepository,
        IGenericRepository<MonthlyPoints> monthlyPointsRepository,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _monthlyPointsRepository = monthlyPointsRepository;
        _currentUserService = currentUserService;
    }

    public async Task<CustomerDetailResult> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra quyền truy cập (Business Logic)
        var currentUserRole = _currentUserService.Role;
        var currentUserId = _currentUserService.UserId;

        // Nếu là Customer, chỉ được xem chính hồ sơ của mình
        if (currentUserRole == "Customer" && currentUserId != request.CustomerId.ToString())
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xem thông tin của người khác.");
        }

        // 2. Lấy thông tin Customer và kiểm tra tồn tại (Data Validation)
        var customer = await _userRepository.FirstOrDefaultAsync(u => u.UserId == request.CustomerId);

        if (customer == null || customer.Role != "Customer")
        {
            throw new KeyNotFoundException($"Không tìm thấy khách hàng #{request.CustomerId}");
        }

        // 3. Thống kê đơn hàng (Sử dụng IQueryable để thực thi tại Database)
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
                (int)(o.Price / 1000), // Quy tắc tích điểm
                o.TimeCreate
            ))
            .ToList();

        // 4. Xếp hạng & Tổng số khách hàng
        var rank = _userRepository.Query()
            .Count(u => u.Role == "Customer" && u.TotalPoint > customer.TotalPoint) + 1;

        var totalCustomers = _userRepository.Query()
            .Count(u => u.Role == "Customer");

        // 5. Điểm tích lũy tháng hiện tại
        var now = DateTime.Now;
        var currentMonthPoints = _monthlyPointsRepository.Query()
            .Where(mp => mp.CustomerId == request.CustomerId
                      && mp.Month == now.Month
                      && mp.Year == now.Year)
            .Select(mp => mp.MonthlyTotal)
            .FirstOrDefault();

        // 6. Trả kết quả DTO
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