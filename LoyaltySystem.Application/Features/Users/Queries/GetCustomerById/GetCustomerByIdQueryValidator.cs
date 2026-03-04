using FluentValidation;

namespace LoyaltySystem.Application.Features.Users.Queries.GetCustomerById;

public class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0).WithMessage("ID khách hàng không hợp lệ.");
    }
}