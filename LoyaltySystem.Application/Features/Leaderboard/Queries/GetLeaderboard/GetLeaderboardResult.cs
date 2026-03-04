namespace LoyaltySystem.Application.Features.Leaderboard.Queries.GetLeaderboard;

public record LeaderboardResult(
    List<LeaderboardItemDto> Items,
    int TotalRecords,
    int PageNumber,
    int PageSize,
    int TotalPages
);

public record LeaderboardItemDto(
    int Rank,
    int UserId,
    string UserName,
    string PhoneNumber,
    int TotalPoints
);