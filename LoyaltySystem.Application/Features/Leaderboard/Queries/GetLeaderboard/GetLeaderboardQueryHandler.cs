using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Interfaces;
using MediatR;
using System.Linq; // Chỉ dùng LINQ chuẩn của C#

namespace LoyaltySystem.Application.Features.Leaderboard.Queries.GetLeaderboard;

public class GetLeaderboardQueryHandler : IRequestHandler<GetLeaderboardQuery, LeaderboardResult>
{
    private readonly IGenericRepository<User> _userRepository;

    public GetLeaderboardQueryHandler(IGenericRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<LeaderboardResult> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        // 1. Lấy Queryable từ Repository
        var query = _userRepository.Query()
            .Where(u => u.Role == "Customer")
            .OrderByDescending(u => u.TotalPoint);

        // 2. Thực hiện các phép toán (Validator đã đảm bảo PageNumber >= 1 và PageSize >= 1)
        var totalRecords = query.Count();

        var pagedCustomers = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // 3. Tính toán Rank và Map sang DTO
        var startRank = (request.PageNumber - 1) * request.PageSize + 1;

        var items = pagedCustomers.Select((customer, index) => new LeaderboardItemDto(
            Rank: startRank + index,
            UserId: customer.UserId,
            UserName: customer.UserName,
            PhoneNumber: customer.PhoneNumber,
            TotalPoints: customer.TotalPoint
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        var result = new LeaderboardResult(
            Items: items,
            TotalRecords: totalRecords,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalPages: totalPages
        );

        // Trả về Task hoàn thành vì Interface IRequestHandler yêu cầu trả về Task
        return Task.FromResult(result);
    }
}