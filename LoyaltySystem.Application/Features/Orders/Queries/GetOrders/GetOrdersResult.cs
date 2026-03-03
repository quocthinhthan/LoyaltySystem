namespace LoyaltySystem.Application.Features.Orders.Queries.GetOrders;

public record GetOrdersResult(
    List<OrderDto> Orders,
    PaginationMetadata Pagination  // ✅ Thông tin phân trang
);

public record OrderDto(
    int OrderId,
    string CustomerName,
    string CustomerPhone,
    decimal Price,
    int PointsEarned,
    DateTime TimeCreate,
    string StaffName
);

// ✅ Thông tin phân trang
public record PaginationMetadata(
    int CurrentPage,
    int PageSize,
    int TotalRecords,
    int TotalPages
);