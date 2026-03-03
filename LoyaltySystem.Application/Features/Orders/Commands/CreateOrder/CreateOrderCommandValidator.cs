using FluentValidation;

namespace LoyaltySystem.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerPhoneNumber)
            .NotEmpty().WithMessage("Số điện thoại khách hàng không được để trống")
            .Matches(@"^(02|03|05|07|08|09)\d{8}$")
            .WithMessage("Số điện thoại không đúng định dạng Việt Nam");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Số tiền phải lớn hơn 0")
            .LessThanOrEqualTo(100_000_000).WithMessage("Số tiền không hợp lệ");

        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Mã nhân viên không được để trống")
            .Must(id => int.TryParse(id, out var result) && result > 0)
            .WithMessage("Mã nhân viên phải là số nguyên dương");
    }
}