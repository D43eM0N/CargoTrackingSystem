using MediatR;

namespace CargoTrackingSystem.Application.Features.Customers.Commands.DeleteCustomer;

public record DeleteCustomerCommand(Guid Id) : IRequest<DeleteCustomerResponse>;

public record DeleteCustomerResponse(string Message);
