using MediatR;

namespace LoyaltySystem.Application.Features.Leaderboard.Queries.GetMonthlyLeaderboard;

public record GetMonthlyLeaderboardQuery  : IRequest<MonthlyLeaderboardResult>
{
    public int? StartMonth { get; init; } = null;
    public int? StartYear { get; init; } = null;
    public int? EndMonth { get; init; } = null;
    public int? EndYear { get; init; } = null;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}