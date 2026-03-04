using FluentValidation;

namespace LoyaltySystem.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Tên người dùng không được để trống.")
            .MaximumLength(50).WithMessage("Tên không được quá 50 ký tự.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Số điện thoại không được để trống.")
            .Matches(@"^(0[3|5|7|8|9])[0-9]{8}$").WithMessage("Số điện thoại không đúng định dạng.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu phải từ 6 ký tự trở lên.");

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => new[] { "Customer", "Staff" }.Contains(role))
            .WithMessage("Quyền hạn (Role) không hợp lệ.");
    }
}