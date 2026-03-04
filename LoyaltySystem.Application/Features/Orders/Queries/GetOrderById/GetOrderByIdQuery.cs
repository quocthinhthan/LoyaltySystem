// GetOrderByIdQuery.cs bây giờ chỉ cần OrderId
using LoyaltySystem.Application.Features.Orders.Queries.GetOrderById;
using MediatR;

public record GetOrderByIdQuery(int OrderId) : IRequest<OrderDetailResult>;