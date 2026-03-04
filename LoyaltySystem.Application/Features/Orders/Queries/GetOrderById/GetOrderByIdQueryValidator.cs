using FluentValidation;

namespace LoyaltySystem.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Mã đơn hàng không hợp lệ.");
    }
}