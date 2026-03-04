namespace LoyaltySystem.Application.Features.Leaderboard.Queries.GetMonthlyLeaderboard;

public record MonthlyLeaderboardResult(
    int StartMonth,
    int StartYear,
    int EndMonth,
    int EndYear,
    List<MonthlyLeaderboardItemDto> Items,
    int TotalRecords,
    int PageNumber,
    int PageSize,
    int TotalPages
);

public record MonthlyLeaderboardItemDto(
    int Rank,
    int UserId,
    string UserName,
    string PhoneNumber,
    int MonthlyPoints,
    int TotalPoints
);