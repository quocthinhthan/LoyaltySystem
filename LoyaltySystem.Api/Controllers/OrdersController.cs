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

    /// <summary>
    /// Lấy danh sách orders với filter và pagination
    /// </summary>
    /// <param name="userId">ID người dùng hiện tại</param>
    /// <param name="role">Role: Customer/Staff/Admin</param>
    /// <param name="startDate">Từ ngày (yyyy-MM-dd)</param>
    /// <param name="endDate">Đến ngày (yyyy-MM-dd)</param>
    /// <param name="pageNumber">Số trang (mặc định 1)</param>
    /// <param name="pageSize">Số records/trang (mặc định 10)</param>
    /// <param name="customerPhone">Tìm theo SĐT khách hàng</param>
    /// <param name="sortBy">Sắp xếp theo: TimeCreate, Price</param>
    /// <param name="isDescending">Giảm dần (true) hay tăng dần (false)</param>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int userId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? customerPhone = null,
        [FromQuery] string sortBy = "TimeCreate",
        [FromQuery] bool isDescending = true)
    {
        var query = new GetOrdersQuery(
            UserId: userId,
            UserQueryId: User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
            Role: User.FindFirst(ClaimTypes.Role)?.Value ?? "",
            StartDate: startDate,
            EndDate: endDate,
            PageNumber: pageNumber,
            PageSize: pageSize,
            CustomerPhone: customerPhone,
            SortBy: sortBy,
            IsDescending: isDescending
        );

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Xem chi tiết 1 order
    /// </summary>
    [HttpGet("{orderId}")]
    [Authorize]
    public async Task<IActionResult> GetOrderById(
        int orderId,
        [FromQuery] int userId,
        [FromQuery] string role = "Customer")
    {
        var query = new GetOrderByIdQuery(orderId, userId, role);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

// DTO Request - Client chỉ cần gửi CustomerPhoneNumber và Price
// StaffId được tự động lấy từ JWT token
public record CreateOrderRequest(
    string CustomerPhoneNumber,
    decimal Price
);