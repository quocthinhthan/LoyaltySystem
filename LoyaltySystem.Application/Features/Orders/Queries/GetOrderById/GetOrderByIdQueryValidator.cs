using FluentValidation;

namespace LoyaltySystem.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("Mã đơn hàng không hợp lệ.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("ID người dùng không được để trống.")
            .Must(id => int.TryParse(id, out _)).WithMessage("ID người dùng phải là định dạng số.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Quyền hạn không được để trống.")
            .Must(role => new[] { "Customer", "Staff", "Admin" }.Contains(role))
            .WithMessage("Quyền hạn không hợp lệ.");
    }
}