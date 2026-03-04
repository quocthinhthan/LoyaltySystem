using FluentValidation;

namespace LoyaltySystem.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Số trang phải từ 1 trở lên.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Kích thước trang phải từ 1 đến 100.");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("ID người dùng không hợp lệ.");

        RuleFor(x => x.UserQueryId)
            .NotEmpty().WithMessage("ID truy vấn không được để trống.")
            .Must(id => int.TryParse(id, out _)).WithMessage("ID truy vấn phải là định dạng số.");

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => new[] { "Customer", "Staff", "Admin" }.Contains(role))
            .WithMessage("Quyền hạn không hợp lệ.");

        // Kiểm tra logic ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Ngày bắt đầu không được lớn hơn ngày kết thúc.");

        RuleFor(x => x.SortBy)
            .Must(sort => string.IsNullOrEmpty(sort) || new[] { "TimeCreate", "Price" }.Contains(sort, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Trường sắp xếp không hợp lệ (chỉ chấp nhận TimeCreate hoặc Price).");
    }
}