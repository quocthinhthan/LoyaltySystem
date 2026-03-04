using LoyaltySystem.Domain.Entities;
using LoyaltySystem.Domain.Interfaces;
using MediatR;

namespace LoyaltySystem.Application.Features.Leaderboard.Queries.GetLeaderboard;

public class GetLeaderboardQueryHandler : IRequestHandler<GetLeaderboardQuery, LeaderboardResult>
{
    private readonly IGenericRepository<User> _userRepository;

    public GetLeaderboardQueryHandler(IGenericRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<LeaderboardResult> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        // 1. Query customers và sắp xếp (trên DB)
        var customers = _userRepository.Query()
            .Where(u => u.Role == "Customer")
            .OrderByDescending(u => u.TotalPoint);

        // 2. Count
        var totalRecords = customers.Count();

        // 3. Pagination
        var pagedCustomers = customers
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // 4. Map sang DTO với rank
        var startRank = (request.PageNumber - 1) * request.PageSize + 1;
        var items = pagedCustomers.Select((customer, index) => new LeaderboardItemDto(
            Rank: startRank + index,
            UserId: customer.UserId,
            UserName: customer.UserName,
            PhoneNumber: customer.PhoneNumber,
            TotalPoints: customer.TotalPoint
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);

        return new LeaderboardResult(
            Items: items,
            TotalRecords: totalRecords,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalPages: totalPages
        );
    }
}

