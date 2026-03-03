using MediatR;

namespace LoyaltySystem.Application.Features.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    string CustomerPhoneNumber,
    decimal Price,
    string StaffId
) : IRequest<CreateOrderResult>;