using FluentValidation;

namespace LoyaltySystem.Application.Features.Users.Queries.GetCustomerById;

public class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("ID khách hàng không hợp lệ.");

        RuleFor(x => x.CurrentUserId)
            .GreaterThan(0).WithMessage("ID người dùng hiện tại không hợp lệ.");

        RuleFor(x => x.CurrentUserRole)
            .NotEmpty().WithMessage("Quyền hạn không được để trống.")
            .Must(role => new[] { "Customer", "Staff", "Admin" }.Contains(role))
            .WithMessage("Quyền hạn không hợp lệ.");
    }
}