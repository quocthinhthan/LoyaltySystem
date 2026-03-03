using LoyaltySystem.Domain.Interfaces;
using MediatR;

namespace LoyaltySystem.Application.Features.Leaderboard.Queries.GetLeaderboard;

public class GetLeaderboardQueryHandler : IRequestHandler<GetLeaderboardQuery, LeaderboardResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetLeaderboardQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<LeaderboardResult> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        // 1. Lấy tất cả customers và sắp xếp theo TotalPoint giảm dần
        var allCustomers = (await _unitOfWork.Users.GetAllAsync())
            .Where(u => u.Role == "Customer")
            .OrderByDescending(u => u.TotalPoint)
            .ToList();

        var totalRecords = allCustomers.Count;

        // 2. Áp dụng pagination
        var customers = allCustomers
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // 3. Map sang DTO với thứ hạng
        var startRank = (request.PageNumber - 1) * request.PageSize + 1;
        var items = customers.Select((customer, index) => new LeaderboardItemDto(
            Rank: startRank + index,
            UserId: customer.UserId,
            UserName: customer.UserName,
            PhoneNumber: customer.PhoneNumber,
            TotalPoints: customer.TotalPoint
        )).ToList();

        return new LeaderboardResult(
            Items: items,
            TotalRecords: totalRecords,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalPages: (int)Math.Ceiling(totalRecords / (double)request.PageSize)
        );
    }
}

// DTOs
public record LeaderboardItemDto(
    int Rank,
    int UserId,
    string UserName,
    string PhoneNumber,
    int TotalPoints
);

public record LeaderboardResult(
    List<LeaderboardItemDto> Items,
    int TotalRecords,
    int PageNumber,
    int PageSize,
    int TotalPages
);