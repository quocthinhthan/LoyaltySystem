namespace LoyaltySystem.Application.Features.Users.Queries.GetStaffById;

public record StaffDetailResult(
    int UserId,
    string UserName,
    string PhoneNumber,
    string Role,
    int TotalOrdersHandled,
    decimal TotalRevenue,
    List<RecentOrderDto> RecentOrders
);

public record RecentOrderDto(
    int OrderId,
    decimal Price,
    DateTime TimeCreate
);