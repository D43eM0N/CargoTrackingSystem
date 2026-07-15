using MediatR;

namespace CargoTrackingSystem.Application.Features.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string FullName,
    string Email,
    string PhoneNumber
) : IRequest<CreateCustomerResponse>;

public record CreateCustomerResponse(string Message, Guid CustomerId);
