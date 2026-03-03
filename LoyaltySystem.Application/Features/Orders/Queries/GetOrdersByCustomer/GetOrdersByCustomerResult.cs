namespace LoyaltySystem.Application.Features.Orders.Queries.GetOrdersByCustomer;

public record GetOrdersByCustomerResult(
    CustomerSummary Customer,
    List<OrderSummaryDto> Orders,
    PaginationInfo Pagination  // ✅ Thêm pagination
);

public record CustomerSummary(
    int UserId,
    string UserName,
    string PhoneNumber,
    int TotalPoints,
    int TotalOrders,       // Tổng orders trong khoảng thời gian filter
    decimal TotalSpent     // Tổng tiền trong khoảng thời gian filter
);

public record OrderSummaryDto(
    int OrderId,
    decimal Price,
    int PointsEarned,
    DateTime TimeCreate
);

public record PaginationInfo(
    int CurrentPage,
    int PageSize,
    int TotalRecords,
    int TotalPages
);