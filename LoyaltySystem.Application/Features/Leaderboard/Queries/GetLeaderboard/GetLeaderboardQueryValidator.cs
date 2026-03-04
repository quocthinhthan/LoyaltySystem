using FluentValidation;

namespace LoyaltySystem.Application.Features.Leaderboard.Queries.GetLeaderboard;

public class GetLeaderboardQueryValidator : AbstractValidator<GetLeaderboardQuery>
{
    public GetLeaderboardQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang phải lớn hơn hoặc bằng 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Kích thước trang phải lớn hơn hoặc bằng 1.")
            .LessThanOrEqualTo(100).WithMessage("Kích thước trang không được vượt quá 100 để đảm bảo hiệu năng.");
    }
}