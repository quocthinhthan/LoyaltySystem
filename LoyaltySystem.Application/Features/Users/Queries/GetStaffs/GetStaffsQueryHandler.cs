using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Interfaces;
using MediatR;
using System.Linq;

namespace LoyaltySystem.Application.Features.Users.Queries.GetStaffs;

public class GetStaffsQueryHandler : IRequestHandler<GetStaffsQuery, GetStaffsResult>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Order> _orderRepository;

    public GetStaffsQueryHandler(
        IGenericRepository<User> userRepository,
        IGenericRepository<Order> orderRepository)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
    }

    public async Task<GetStaffsResult> Handle(GetStaffsQuery request, CancellationToken cancellationToken)
    {
        // 1. Khởi tạo Query
        var users = _userRepository.Query();
        var orders = _orderRepository.Query();

        // 2. Lọc Role Staff và SearchTerm
        var staffQuery = users.Where(u => u.Role == "Staff");

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            staffQuery = staffQuery.Where(u =>
                (u.UserName != null && u.UserName.ToLower().Contains(search)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));
        }

        // 3. Sorting (Mặc định theo tên cho Staff)
        staffQuery = staffQuery.OrderBy(u => u.UserName);

        // 4. Count & Pagination
        // Bỏ qua các bước kiểm tra <= 0 vì Validator đã đảm bảo dữ liệu đúng
        var totalRecords = staffQuery.Count();

        var pagedStaffs = staffQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // 5. Lấy thống kê số đơn hàng đã xử lý (Batch Query)
        var staffIds = pagedStaffs.Select(s => s.UserId).ToList();

        var orderCounts = orders
            .Where(o => staffIds.Contains(o.StaffId))
            .GroupBy(o => o.StaffId)
            .Select(g => new {
                StaffId = g.Key,
                Count = g.Count()
            })
            .ToDictionary(x => x.StaffId, x => x.Count);

        // 6. Map DTO
        var staffDtos = pagedStaffs.Select(s => new StaffDto(
            s.UserId,
            s.UserName ?? "Unknown",
            s.PhoneNumber ?? "N/A",
            orderCounts.TryGetValue(s.UserId, out var count) ? count : 0
        )).ToList();

        // 7. Kết quả
        int totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

        return new GetStaffsResult(
            staffDtos,
            totalRecords,
            request.PageNumber,
            request.PageSize,
            totalPages
        );
    }
}