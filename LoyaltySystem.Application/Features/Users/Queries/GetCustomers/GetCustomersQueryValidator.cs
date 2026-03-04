using FluentValidation;

namespace LoyaltySystem.Application.Features.Users.Queries.GetCustomers;

public class GetCustomersQueryValidator : AbstractValidator<GetCustomersQuery>
{
    public GetCustomersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang phải từ 1 trở lên.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100.");

        RuleFor(x => x.SortBy)
            .Must(sort => string.IsNullOrEmpty(sort) || new[] { "username", "totalpoint" }.Contains(sort.ToLower()))
            .WithMessage("Trường sắp xếp không hợp lệ (chỉ chấp nhận username hoặc totalpoint).");
    }
}