using FluentValidation;

namespace LoyaltySystem.Application.Features.Users.Queries.GetStaffById;

public class GetStaffByIdQueryValidator : AbstractValidator<GetStaffByIdQuery>
{
    public GetStaffByIdQueryValidator()
    {
        RuleFor(x => x.StaffId)
            .GreaterThan(0).WithMessage("ID nhân viên không hợp lệ.");

        RuleFor(x => x.CurrentUserId)
            .GreaterThan(0).WithMessage("ID người dùng hiện tại không hợp lệ.");

        RuleFor(x => x.CurrentUserRole)
            .NotEmpty().WithMessage("Quyền hạn không được để trống.")
            .Must(role => new[] { "Customer", "Staff", "Admin" }.Contains(role))
            .WithMessage("Quyền hạn không hợp lệ.");
    }
}