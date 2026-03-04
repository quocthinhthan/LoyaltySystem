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
        int endMonth = request.EndMonth ?? now.Month;
        int endYear = request.EndYear ?? now.Year;
        int startMonth = request.StartMonth ?? endMonth;
        int startYear = request.StartYear ?? endYear;

        // 1. Chỉ định nghĩa Query, chưa tải dữ liệu về RAM
        // Sử dụng IQueryable để SQL Server xử lý lọc và tính toán
        var query = _unitOfWork.MonthlyPoints.Query()
            .Where(mp => (mp.Year > startYear || (mp.Year == startYear && mp.Month >= startMonth)) &&
                         (mp.Year < endYear || (mp.Year == endYear && mp.Month <= endMonth)));

        // 2. Thực hiện GroupBy và Sum ngay tại Database
        var statsQuery = query
            .GroupBy(mp => mp.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                TotalMonthlyPoints = g.Sum(mp => mp.MonthlyTotal)
            })
            .OrderByDescending(x => x.TotalMonthlyPoints);

        // 3. Đếm tổng số bản ghi bằng SQL Count
        var totalRecords = statsQuery.Count();

        // 4. Phân trang tại Database (Chỉ lấy đúng số PageSize bản ghi)
        var pagedData = statsQuery
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // 5. Chỉ lấy thông tin User của những người có trong trang này
        var customerIds = pagedData.Select(x => x.CustomerId).ToList();
        var users = _unitOfWork.Users.Query()
            .Where(u => customerIds.Contains(u.UserId))
            .ToDictionary(u => u.UserId);

        // 6. Map kết quả cuối cùng
        var startRank = (request.PageNumber - 1) * request.PageSize + 1;
        var items = pagedData.Select((item, index) =>
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
            StartMonth: startMonth, StartYear: startYear,
            EndMonth: endMonth, EndYear: endYear,
            Items: items,
            TotalRecords: totalRecords,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalPages: (int)Math.Ceiling(totalRecords / (double)request.PageSize)
        );
    }

}