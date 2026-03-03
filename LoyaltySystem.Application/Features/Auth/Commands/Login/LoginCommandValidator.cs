using FluentValidation;

namespace LoyaltySystem.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(v => v.PhoneNumber)
            .NotEmpty().WithMessage("Số điện thoại không được để trống.")
            .Matches(@"^(0[2|   3|5|7|8|9])+([0-9]{8})$").WithMessage("Số điện thoại không hợp lệ.");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu phải từ 6 ký tự trở lên.");
    }
}