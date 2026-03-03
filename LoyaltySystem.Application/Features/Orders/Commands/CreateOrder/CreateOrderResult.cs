namespace LoyaltySystem.Application.Features.Orders.Commands.CreateOrder;

public record CreateOrderResult(
    int OrderId,
    string CustomerName,
    decimal Price,
    int PointsEarned,
    int TotalPoints,
    string Message
);