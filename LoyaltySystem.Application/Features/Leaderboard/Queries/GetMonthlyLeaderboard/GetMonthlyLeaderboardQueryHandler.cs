using LoyaltySystem.Domain.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        // 1. Xử lý giá trị mặc định (Đây là logic khởi tạo, không phải validate)
        int endMonth = request.EndMonth ?? now.Month;
        int endYear = request.EndYear ?? now.Year;
        int startMonth = request.StartMonth ?? endMonth;
        int startYear = request.StartYear ?? endYear;

        // 2. Logic nghiệp vụ: Kiểm tra ngày bắt đầu không được lớn hơn ngày kết thúc
        var startDate = new DateTime(startYear, startMonth, 1);
        var endDate = new DateTime(endYear, endMonth, 1);

        if (startDate > endDate)
        {
            throw new Exception("Khoảng thời gian bắt đầu phải trước hoặc bằng thời gian kết thúc.");
        }

        // 3. Lấy dữ liệu (Dùng IUnitOfWork để tránh phụ thuộc EF Core trực tiếp tại đây)
        var allMonthlyPoints = (await _unitOfWork.MonthlyPoints.GetAllAsync())
            .Where(mp => IsInRange(mp.Month, mp.Year, startMonth, startYear, endMonth, endYear))
            .ToList();

        // 4. Group by CustomerId và tính tổng điểm
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

        // 5. Áp dụng phân trang (PageNumber và PageSize đã được Validator đảm bảo >= 1)
        var pagedCustomerPoints = customerPointsMap
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // 6. Lấy thông tin User tương ứng
        var customerIds = pagedCustomerPoints.Select(x => x.CustomerId).ToList();
        var users = (await _unitOfWork.Users.GetAllAsync())
            .Where(u => customerIds.Contains(u.UserId))
            .ToDictionary(u => u.UserId);

        // 7. Map sang DTO với thứ hạng (Rank)
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

    private bool IsInRange(int month, int year, int startMonth, int startYear, int endMonth, int endYear)
    {
        var date = new DateTime(year, month, 1);
        var startDate = new DateTime(startYear, startMonth, 1);
        var endDate = new DateTime(endYear, endMonth, 1);

        return date >= startDate && date <= endDate;
    }
}