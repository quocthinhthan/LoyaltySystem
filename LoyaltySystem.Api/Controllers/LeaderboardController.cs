using LoyaltySystem.Application.Features.Leaderboard.Queries.GetLeaderboard;
using LoyaltySystem.Application.Features.Leaderboard.Queries.GetMonthlyLeaderboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LoyaltySystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeaderboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy bảng xếp hạng tổng điểm (TotalPoint)
    /// </summary>
    /// <param name="pageNumber">Số trang (mặc định 1)</param>
    /// <param name="pageSize">Số records/trang (mặc định 50)</param>
    [HttpGet]
    public async Task<IActionResult> GetLeaderboard(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new GetLeaderboardQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Lấy bảng xếp hạng theo khoảng thời gian (MonthlyPoints)
    /// </summary>
    /// <param name="startMonth">Tháng bắt đầu (1-12, mặc định = endMonth)</param>
    /// <param name="startYear">Năm bắt đầu (VD: 2026, mặc định = endYear)</param>
    /// <param name="endMonth">Tháng kết thúc (1-12, mặc định = tháng hiện tại)</param>
    /// <param name="endYear">Năm kết thúc (VD: 2026, mặc định = năm hiện tại)</param>
    /// <param name="pageNumber">Số trang (mặc định 1)</param>
    /// <param name="pageSize">Số records/trang (mặc định 50)</param>
    /// <remarks>
    /// Ví dụ:
    /// - Xem tháng hiện tại: không truyền gì
    /// - Xem tháng 3/2026: ?endMonth=3&amp;endYear=2026
    /// - Xem từ 1/2026 đến 3/2026: ?startMonth=1&amp;startYear=2026&amp;endMonth=3&amp;endYear=2026
    /// - Xem cả năm 2026: ?startMonth=1&amp;startYear=2026&amp;endMonth=12&amp;endYear=2026
    /// </remarks>
    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlyLeaderboard(
        [FromQuery] int? startMonth = null,
        [FromQuery] int? startYear = null,
        [FromQuery] int? endMonth = null,
        [FromQuery] int? endYear = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new GetMonthlyLeaderboardQuery(
            startMonth,
            startYear,
            endMonth,
            endYear,
            pageNumber,
            pageSize
        );
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}