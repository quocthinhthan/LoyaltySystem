namespace LoyaltySystem.Application.Features.Users.Queries.GetCustomerById;

public record CustomerDetailResult(
    int UserId,
    string UserName,
    string PhoneNumber,
    int TotalPoint,
    int Rank,
    int TotalCustomers,
    int TotalOrders,
    decimal TotalSpent,
    int CurrentMonthPoints,
    List<RecentOrderDto> RecentOrders
);

public record RecentOrderDto(
    int OrderId,
    decimal Price,
    int PointsEarned,
    DateTime TimeCreate
);