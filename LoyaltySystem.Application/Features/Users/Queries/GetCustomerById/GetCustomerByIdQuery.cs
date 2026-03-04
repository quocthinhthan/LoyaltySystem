using MediatR;

namespace LoyaltySystem.Application.Features.Users.Queries.GetCustomerById;

public record GetCustomerByIdQuery(
    int CustomerId, 
    int CurrentUserId, 
    string CurrentUserRole
) : IRequest<CustomerDetailResult>;
