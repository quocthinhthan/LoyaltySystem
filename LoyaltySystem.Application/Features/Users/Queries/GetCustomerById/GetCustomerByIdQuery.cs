using MediatR;

namespace LoyaltySystem.Application.Features.Users.Queries.GetCustomerById;

public record GetCustomerByIdQuery(int CustomerId) : IRequest<CustomerDetailResult>;