namespace LoyaltySystem.Application.Features.Users.Queries.GetCustomers;

public record GetCustomersResult(
    List<CustomerDto> Items,
    int TotalRecords,
    int PageNumber,
    int PageSize,
    int TotalPages
);

public record CustomerDto(
    int UserId,
    string UserName,
    string PhoneNumber,
    int TotalPoint,
    int TotalOrders,
    decimal TotalSpent
);