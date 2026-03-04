using MediatR;

namespace LoyaltySystem.Application.Features.Users.Queries.GetStaffById;

public record GetStaffByIdQuery(
    int StaffId,
    int CurrentUserId,
    string CurrentUserRole
) : IRequest<StaffDetailResult>;