namespace LoyaltySystem.Application.Features.Orders.Queries.GetOrderById;

public record OrderDetailResult(
    int OrderId,
    CustomerInfo Customer,
    StaffInfo Staff,
    decimal Price,
    int PointsEarned,
    DateTime TimeCreate
);

public record CustomerInfo(
    int UserId,
    string UserName,
    string PhoneNumber,
    int TotalPoints
);

public record StaffInfo(
    int UserId,
    string UserName
);