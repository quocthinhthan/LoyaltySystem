using LoyaltySystem.Application.Features.Orders.Commands.CreateOrder;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    [Authorize]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var staffId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(staffId))
        {
            return Unauthorized(new { message = "Không tìm thấy thông tin Staff" });
        }
        var command = new CreateOrderCommand(
            CustomerPhoneNumber: request.CustomerPhoneNumber,
            Price: request.Price,
            StaffId: staffId
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