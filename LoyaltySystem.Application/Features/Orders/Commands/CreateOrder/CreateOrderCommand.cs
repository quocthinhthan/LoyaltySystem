using MediatR;

namespace LoyaltySystem.Application.Features.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    string CustomerPhoneNumber,
    decimal Price,
    int StaffId // Tạm thời hardcode, sau sẽ lấy từ JWT
) : IRequest<CreateOrderResult>;