using MediatR;

namespace LoyaltySystem.Application.Features.Orders.Queries.GetOrders;

public record GetOrdersQuery(
    int UserId,                 // Người gọi API
    string Role,                // Role để phân quyền
    DateTime? StartDate,        // ✅ Filter từ ngày (nullable)
    DateTime? EndDate,          // ✅ Filter đến ngày (nullable)
    int PageNumber = 1,         // ✅ Số trang (mặc định 1)
    int PageSize = 10,          // ✅ Số records/trang (mặc định 10)
    string? CustomerPhone = null, // ✅ Tìm theo SĐT khách hàng (optional)
    string SortBy = "TimeCreate", // ✅ Sort theo field (TimeCreate, Price)
    bool IsDescending = true     // ✅ Sắp xếp giảm dần
) : IRequest<GetOrdersResult>;