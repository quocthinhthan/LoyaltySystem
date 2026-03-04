using MediatR;

namespace LoyaltySystem.Application.Features.Users.Queries.GetStaffs;

public record GetStaffsQuery(
    string? SearchTerm,
    int PageNumber,
    int PageSize
) : IRequest<GetStaffsResult>;  