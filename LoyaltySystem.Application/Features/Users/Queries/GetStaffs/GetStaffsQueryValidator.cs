using FluentValidation;

namespace LoyaltySystem.Application.Features.Users.Queries.GetStaffs;

public class GetStaffsQueryValidator : AbstractValidator<GetStaffsQuery>
{
    public GetStaffsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang phải từ 1 trở lên.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("Từ khóa tìm kiếm không được quá 100 ký tự.");
    }
}