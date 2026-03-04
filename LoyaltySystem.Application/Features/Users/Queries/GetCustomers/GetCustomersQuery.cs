using MediatR;

namespace LoyaltySystem.Application.Features.Users.Queries.GetCustomers;

public record GetCustomersQuery(
    string? SearchTerm,
    string? SortBy,
    bool IsDescending,
    int PageNumber,
    int PageSize
) : IRequest<GetCustomersResult>;