//using LoyaltySystem.Application.Features.Orders.Commands.CreateOrder;
//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//[Authorize] // Cần Token để gọi
//[ApiController]
//[Route("api/[controller]")]
//public class OrdersController : ControllerBase
//{
//    private readonly IMediator _mediator;
//    public OrdersController(IMediator mediator) => _mediator = mediator;

//    [HttpPost]
//    [Authorize(Roles = "Staff,Admin")]
//    public async Task<ActionResult<OrderCreatedResult>> Create(CreateOrderCommand command)
//    {
//        return await _mediator.Send(command); // Gọi CreateOrderCommandHandler
//    }
//}