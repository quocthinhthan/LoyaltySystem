using MediatR;

namespace LoyaltySystem.Application.Features.Leaderboard.Queries.GetLeaderboard;

public record GetLeaderboardQuery(
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<LeaderboardResult>;