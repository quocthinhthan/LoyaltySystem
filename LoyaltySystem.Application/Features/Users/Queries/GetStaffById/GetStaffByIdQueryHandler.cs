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
        // 1. Lấy thông tin nhân viên qua Interface Repository
        var staff = await _userRepository.FirstOrDefaultAsync(u => u.UserId == request.StaffId);

        if (staff == null || staff.Role != "Staff")
        {
            throw new KeyNotFoundException($"Không tìm thấy nhân viên #{request.StaffId}");
        }

        // 2. Tạo Queryable cho đơn hàng
        var orders = _orderRepository.Query()
            .Where(o => o.StaffId == request.StaffId);

        // 3. Thực hiện thống kê (Sử dụng Linq tiêu chuẩn)
        var totalOrders = orders.Count();
        var totalRevenue = orders.Sum(o => o.Price);

        // 4. Lấy danh sách đơn hàng gần đây
        // FIX CS0853: Loại bỏ tên tham số (OrderId:, Price:...) trong new RecentOrderDto
        var recentOrders = orders
            .OrderByDescending(o => o.TimeCreate)
            .Take(5)
            .Select(o => new RecentOrderDto(
                o.OrderId,
                o.Price,
                o.TimeCreate
            ))
            .ToList();

        // 5. Trả về kết quả
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