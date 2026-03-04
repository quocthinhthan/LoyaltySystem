using FluentValidation;

namespace LoyaltySystem.Application.Features.Leaderboard.Queries.GetMonthlyLeaderboard;

public class GetMonthlyLeaderboardQueryValidator : AbstractValidator<GetMonthlyLeaderboardQuery>
{
    public GetMonthlyLeaderboardQueryValidator()
    {
        RuleFor(x => x.StartMonth)
            .InclusiveBetween(1, 12).When(x => x.StartMonth.HasValue)
            .WithMessage("Tháng bắt đầu phải từ 1 đến 12.");

        RuleFor(x => x.EndMonth)
            .InclusiveBetween(1, 12).When(x => x.EndMonth.HasValue)
            .WithMessage("Tháng kết thúc phải từ 1 đến 12.");

        RuleFor(x => x.StartYear)
            .InclusiveBetween(2000, 2100).When(x => x.StartYear.HasValue)
            .WithMessage("Năm bắt đầu không hợp lệ.");

        RuleFor(x => x.EndYear)
            .InclusiveBetween(2000, 2100).When(x => x.EndYear.HasValue)
            .WithMessage("Năm kết thúc không hợp lệ.");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang phải lớn hơn hoặc bằng 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100.");
    }
}