using MediatR;

namespace LoyaltySystem.Application.Features.Orders.Queries.GetOrdersByCustomer;

public record GetOrdersByCustomerQuery(
    int CustomerId,             // Customer cần xem
    string CallerRole,          // Người gọi phải là Staff/Admin
    DateTime? StartDate = null, // ✅ Filter từ ngày
    DateTime? EndDate = null,   // ✅ Filter đến ngày
    int PageNumber = 1,         // ✅ Phân trang
    int PageSize = 20           // ✅ Số records/trang
) : IRequest<GetOrdersByCustomerResult>;