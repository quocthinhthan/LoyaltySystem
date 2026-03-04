using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Interfaces;
using MediatR;
using System.Linq;

namespace LoyaltySystem.Application.Features.Users.Queries.GetStaffById;

public class GetStaffByIdQueryHandler : IRequestHandler<GetStaffByIdQuery, StaffDetailResult>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Order> _orderRepository;

    public GetStaffByIdQueryHandler(
        IGenericRepository<User> userRepository,
        IGenericRepository<Order> orderRepository)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
    }

    public async Task<StaffDetailResult> Handle(GetStaffByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra quyền (Business Logic)
        if (request.CurrentUserRole == "Staff" && request.CurrentUserId != request.StaffId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xem thông tin của nhân viên khác.");
        }

        // 2. Lấy thông tin nhân viên và kiểm tra tồn tại
        var staff = await _userRepository.FirstOrDefaultAsync(u => u.UserId == request.StaffId);

        // Sửa logic: Nhân viên phải có role Staff hoặc Admin
        if (staff == null || (staff.Role != "Staff" && staff.Role != "Admin"))
        {
            throw new KeyNotFoundException($"Không tìm thấy nhân viên #{request.StaffId}");
        }

        // 3. Truy vấn thống kê đơn hàng
        var ordersQuery = _orderRepository.Query()
            .Where(o => o.StaffId == request.StaffId);

        var totalOrders = ordersQuery.Count();
        var totalRevenue = ordersQuery.Sum(o => o.Price);

        // 4. Lấy danh sách đơn hàng gần đây (Top 5)
        var recentOrders = ordersQuery
            .OrderByDescending(o => o.TimeCreate)
            .Take(5)
            .Select(o => new RecentOrderDto(
                o.OrderId,
                o.Price,
                o.TimeCreate
            ))
            .ToList();

        // 5. Trả về kết quả DTO
        return new StaffDetailResult(
            staff.UserId,
            staff.UserName,
            staff.PhoneNumber,
            staff.Role,
            totalOrders,
            totalRevenue,
            recentOrders
        );
    }
}