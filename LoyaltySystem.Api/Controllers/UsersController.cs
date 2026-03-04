using LoyaltySystem.Application.Features.Users.Queries.GetCustomerById;
using LoyaltySystem.Application.Features.Users.Queries.GetCustomers;
using LoyaltySystem.Application.Features.Users.Queries.GetStaffById;
using LoyaltySystem.Application.Features.Users.Queries.GetStaffs;
using MediatR;
using Microsoft.AspNetCore.Authorization; // Thêm namespace này
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoyaltySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    // ========== ADMIN ENDPOINTS ==========

    /// <summary>
    /// [Admin] Xem danh sách nhân viên
    /// </summary>
    [HttpGet("staffs")]
    [Authorize(Policy = "AdminOnly")] // Sử dụng Policy AdminOnly từ Program.cs
    public async Task<IActionResult> GetStaffs([FromQuery] GetStaffsQuery query, CancellationToken ct)
        => Ok(await _mediator.Send(query, ct));

    /// <summary>
    /// [Admin] Xem chi tiết một nhân viên
    /// </summary>
    [HttpGet("staffs/{staffId}")]
    [Authorize(Policy = "StaffOrAdmin")] // Đổi từ AdminOnly sang StaffOrAdmin
    public async Task<IActionResult> GetStaffById(int staffId, CancellationToken ct)
    {
        // Lấy thông tin từ Token đã được giải mã
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

        var query = new GetStaffByIdQuery(staffId, currentUserId, currentUserRole);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    // ========== STAFF & ADMIN ENDPOINTS ==========

    /// <summary>
    /// [Staff/Admin] Xem danh sách khách hàng
    /// </summary>
    [HttpGet("customers")]
    [Authorize(Policy = "StaffOrAdmin")] // Sử dụng Policy StaffOrAdmin từ Program.cs
    public async Task<IActionResult> GetCustomers([FromQuery] GetCustomersQuery query, CancellationToken ct)
        => Ok(await _mediator.Send(query, ct));

    /// <summary>
    /// [Staff/Admin] Xem chi tiết hồ sơ khách hàng
    /// </summary>
    [HttpGet("customers/{customerId}")]
    [Authorize(Policy = "AllRoll")]
    public async Task<IActionResult> GetCustomerById(int customerId, CancellationToken ct)
    {
        // Lấy ID và Role từ Claims đã được AuthenticationMiddleware giải mã
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

        var query = new GetCustomerByIdQuery(customerId, currentUserId, currentUserRole);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }
}