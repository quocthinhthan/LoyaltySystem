using LoyaltySystem.Domain.Interfaces;
using MediatR;

namespace LoyaltySystem.Application.Features.Leaderboard.Queries.GetMonthlyLeaderboard;

public class GetMonthlyLeaderboardQueryHandler : IRequestHandler<GetMonthlyLeaderboardQuery, MonthlyLeaderboardResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMonthlyLeaderboardQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MonthlyLeaderboardResult> Handle(GetMonthlyLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;

        // 1. Xử lý default values
        int endMonth = request.EndMonth ?? now.Month;
        int endYear = request.EndYear ?? now.Year;
        int startMonth = request.StartMonth ?? endMonth;
        int startYear = request.StartYear ?? endYear;

        // 2. Validate
        if (startMonth < 1 || startMonth > 12 || endMonth < 1 || endMonth > 12)
        {
            throw new ArgumentException("Tháng phải từ 1 đến 12");
        }

        if (startYear < 2000 || startYear > 2100 || endYear < 2000 || endYear > 2100)
        {
            throw new ArgumentException("Năm không hợp lệ");
        }

        // 3. Validate start <= end
        var startDate = new DateTime(startYear, startMonth, 1);
        var endDate = new DateTime(endYear, endMonth, 1);
        
        if (startDate > endDate)
        {
            throw new ArgumentException("Tháng/năm bắt đầu phải <= tháng/năm kết thúc");
        }

        // 4. Lấy tất cả MonthlyPoints trong khoảng thời gian
        var allMonthlyPoints = (await _unitOfWork.MonthlyPoints.GetAllAsync())
            .Where(mp => IsInRange(mp.Month, mp.Year, startMonth, startYear, endMonth, endYear))
            .ToList();

        // 5. Group by CustomerId và tính tổng điểm
        var customerPointsMap = allMonthlyPoints
            .GroupBy(mp => mp.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                TotalMonthlyPoints = g.Sum(mp => mp.MonthlyTotal)
            })
            .OrderByDescending(x => x.TotalMonthlyPoints)
            .ToList();

        var totalRecords = customerPointsMap.Count;

        // 6. Áp dụng pagination
        var pagedCustomerPoints = customerPointsMap
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // 7. Lấy thông tin User tương ứng
        var customerIds = pagedCustomerPoints.Select(x => x.CustomerId).ToList();
        var users = (await _unitOfWork.Users.GetAllAsync())
            .Where(u => customerIds.Contains(u.UserId))
            .ToDictionary(u => u.UserId);

        // 8. Map sang DTO với thứ hạng
        var startRank = (request.PageNumber - 1) * request.PageSize + 1;
        var items = pagedCustomerPoints.Select((item, index) =>
        {
            var user = users.GetValueOrDefault(item.CustomerId);
            return new MonthlyLeaderboardItemDto(
                Rank: startRank + index,
                UserId: item.CustomerId,
                UserName: user?.UserName ?? "Unknown",
                PhoneNumber: user?.PhoneNumber ?? "",
                MonthlyPoints: item.TotalMonthlyPoints,
                TotalPoints: user?.TotalPoint ?? 0
            );
        }).ToList();

        return new MonthlyLeaderboardResult(
            StartMonth: startMonth,
            StartYear: startYear,
            EndMonth: endMonth,
            EndYear: endYear,
            Items: items,
            TotalRecords: totalRecords,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalPages: (int)Math.Ceiling(totalRecords / (double)request.PageSize)
        );
    }

    /// <summary>
    /// Kiểm tra tháng/năm có nằm trong khoảng không
    /// </summary>
    private bool IsInRange(int month, int year, int startMonth, int startYear, int endMonth, int endYear)
    {
        var date = new DateTime(year, month, 1);
        var startDate = new DateTime(startYear, startMonth, 1);
        var endDate = new DateTime(endYear, endMonth, 1);

        return date >= startDate && date <= endDate;
    }
}

// DTOs
public record MonthlyLeaderboardItemDto(
    int Rank,
    int UserId,
    string UserName,
    string PhoneNumber,
    int MonthlyPoints,
    int TotalPoints
);

public record MonthlyLeaderboardResult(
    int StartMonth,
    int StartYear,
    int EndMonth,
    int EndYear,
    List<MonthlyLeaderboardItemDto> Items,
    int TotalRecords,
    int PageNumber,
    int PageSize,
    int TotalPages
);