using MediatR;

namespace LoyaltySystem.Application.Features.Leaderboard.Queries.GetMonthlyLeaderboard;

public record GetMonthlyLeaderboardQuery(
    int? StartMonth = null,  // Nếu null thì = EndMonth
    int? StartYear = null,   // Nếu null thì = EndYear
    int? EndMonth = null,    // Nếu null thì lấy tháng hiện tại
    int? EndYear = null,     // Nếu null thì lấy năm hiện tại
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<MonthlyLeaderboardResult>;