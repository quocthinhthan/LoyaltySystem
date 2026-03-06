using LoyaltySystem.Application.Features.Orders.Commands.CreateOrder;
using LoyaltySystem.Application.Features.Orders.Queries.GetOrders;
using LoyaltySystem.Application.Features.Orders.Queries.GetOrderById;
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
    /// Tạo đơn hàng mới
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "StaffOrAdmin")] // Dùng Policy thay vì check thủ công
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand request)
    {
        return Ok(await _mediator.Send(request));
    }

    /// <summary>
    /// Lấy danh sách orders với filter và pagination
    /// </summary>
    /// <param name="startDate">Từ ngày (yyyy-MM-dd)</param>
    /// <param name="endDate">Đến ngày (yyyy-MM-dd)</param>
    /// <param name="pageNumber">Số trang (mặc định 1)</param>
    /// <param name="pageSize">Số records/trang (mặc định 10)</param>
    /// <param name="customerPhone">Tìm theo SĐT khách hàng</param>
    /// <param name="sortBy">Sắp xếp theo: TimeCreate, Price</param>
    /// <param name="isDescending">Giảm dần (true) hay tăng dần (false)</param>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetOrders([FromQuery] GetOrdersQuery query)
    {
        // Chỉ cần gửi query, các thông tin UserId, Role bên trong Query Record nên được xóa bỏ
        return Ok(await _mediator.Send(query));
    }

    /// <summary>
    /// Xem chi tiết 1 order
    /// </summary>
    [HttpGet("{orderId}")]
    [Authorize]
    public async Task<IActionResult> GetOrderById(int orderId)
    {
        return Ok(await _mediator.Send(new GetOrderByIdQuery(orderId)));
    }
}
