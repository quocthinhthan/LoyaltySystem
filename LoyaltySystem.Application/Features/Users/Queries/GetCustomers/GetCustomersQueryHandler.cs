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
        // 1. Query database (Sử dụng IQueryable để tối ưu truy vấn)
        var users = _userRepository.Query();
        var orders = _orderRepository.Query();

        // 2. Lọc Role và SearchTerm
        var customerQuery = users.Where(u => u.Role == "Customer");

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim().ToLower();
            customerQuery = customerQuery.Where(u =>
                (u.UserName != null && u.UserName.ToLower().Contains(search)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));
        }

        // 3. Sorting (Validator đã lọc các field hợp lệ)
        string sortBy = (request.SortBy ?? "totalpoint").ToLower();
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

        // 4. Count & Pagination (Mặc định PageNumber và PageSize đã hợp lệ nhờ Validator)
        var totalRecords = customerQuery.Count();

        var pagedCustomers = customerQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // 5. Thống kê liên quan (Batch Query)
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

        // 6. Map sang DTO
        var customerDtos = pagedCustomers.Select(c => new CustomerDto(
            c.UserId,
            c.UserName ?? "Unknown",
            c.PhoneNumber ?? "N/A",
            c.TotalPoint,
            orderStats.TryGetValue(c.UserId, out var stats) ? stats.Count : 0,
            orderStats.TryGetValue(c.UserId, out var s) ? s.Total : 0
        )).ToList();

        // 7. Trả kết quả phân trang
        int totalPages = (int)Math.Ceiling((double)totalRecords / request.PageSize);

        return new GetCustomersResult(
            customerDtos,
            totalRecords,
            request.PageNumber,
            request.PageSize,
            totalPages
        );
    }
}