using MediatR;

namespace LoyaltySystem.Application.Features.Leaderboard.Queries.GetMonthlyLeaderboard;

public class GetMonthlyLeaderboardQuery : IRequest<MonthlyLeaderboardResult>
{
    public int? StartMonth { get; set; }
    public int? StartYear { get; set; }
    public int? EndMonth { get; set; }
    public int? EndYear { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}