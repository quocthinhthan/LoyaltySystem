using MediatR;

namespace LoyaltySystem.Application.Features.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(
    int OrderId,
    string UserId,      // Người gọi API
    string Role      // Để kiểm tra quyền
) : IRequest<OrderDetailResult>;