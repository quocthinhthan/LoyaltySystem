using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Interfaces;
using MediatR;
using System.Linq;

namespace LoyaltySystem.Application.Features.Users.Queries.GetCustomers;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, GetCustomersResult>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Order> _orderRepository;

    public GetCustomersQueryHandler(
        IGenericRepository<User> userRepository,
        IGenericRepository<Order> orderRepository)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
    }

    public async Task<GetCustomersResult> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        // ===== 1. Kiểm tra quyền (Nếu cần) =====
        // Thường danh sách khách hàng chỉ dành cho Staff hoặc Admin

        // ===== 2. Query database =====
        var users = _userRepository.Query();
        var orders = _orderRepository.Query();

        // ===== 3. Lọc Role và SearchTerm =====
        var customerQuery = users.Where(u => u.Role == "Customer");

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            customerQuery = customerQuery.Where(u =>
                (u.UserName != null && u.UserName.ToLower().Contains(search)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));
        }

        // ===== 4. Sorting =====
        string sortBy = request.SortBy?.ToLower() ?? "totalpoint";
        if (sortBy == "username")
        {
            customerQuery = request.IsDescending
                ? customerQuery.OrderByDescending(u => u.UserName)
                : customerQuery.OrderBy(u => u.UserName);
        }
        else
        {
            customerQuery = request.IsDescending
                ? customerQuery.OrderByDescending(u => u.TotalPoint)
                : customerQuery.OrderBy(u => u.TotalPoint);
        }

        // ===== 5. Count & Pagination logic =====
        var totalRecords = customerQuery.Count();
        int pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        int pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var pagedCustomers = customerQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // ===== 6. Lấy dữ liệu thống kê liên quan (Batch Query) =====
        var customerIds = pagedCustomers.Select(c => c.UserId).ToList();

        var orderStats = orders
            .Where(o => customerIds.Contains(o.CustomerId))
            .GroupBy(o => o.CustomerId)
            .Select(g => new {
                CustomerId = g.Key,
                Count = g.Count(),
                Total = g.Sum(o => o.Price)
            })
            .ToDictionary(x => x.CustomerId);

        // ===== 7. Map DTO =====
        var customerDtos = pagedCustomers.Select(c => new CustomerDto(
            c.UserId,
            c.UserName ?? "Unknown",
            c.PhoneNumber ?? "N/A",
            c.TotalPoint,
            orderStats.TryGetValue(c.UserId, out var stats) ? stats.Count : 0,
            orderStats.TryGetValue(c.UserId, out var s) ? s.Total : 0
        )).ToList();

        // ===== 8. Kết quả =====
        int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

        return new GetCustomersResult(
            customerDtos,
            totalRecords,
            pageNumber,
            pageSize,
            totalPages
        );
    }
}