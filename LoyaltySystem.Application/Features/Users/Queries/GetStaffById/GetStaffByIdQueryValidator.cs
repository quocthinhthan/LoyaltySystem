using FluentValidation;

namespace LoyaltySystem.Application.Features.Users.Queries.GetStaffById;

public class GetStaffByIdQueryValidator : AbstractValidator<GetStaffByIdQuery>
{
    public GetStaffByIdQueryValidator()
    {
        RuleFor(x => x.StaffId)
            .GreaterThan(0).WithMessage("ID nhân viên không hợp lệ.");
    }
}