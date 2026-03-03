using LoyaltySystem.Application.Features.Orders.Commands.CreateOrder;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Tạo đơn hàng và tích điểm cho khách hàng
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var command = new CreateOrderCommand(
            CustomerPhoneNumber: request.CustomerPhoneNumber,
            Price: request.Price,
            StaffId: 1 // Tạm hardcode, sau sẽ lấy từ JWT
        );

        var result = await _mediator.Send(command);

        return Ok(result);
    }
}

// DTO để nhận từ client (không bao gồm StaffId)
public record CreateOrderRequest(
    string CustomerPhoneNumber,
    decimal Price
);