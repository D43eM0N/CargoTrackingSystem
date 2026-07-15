using MediatR;
using CargoTrackingSystem.Domain.Entities;

namespace CargoTrackingSystem.Application.Features.Customers.Queries.GetCustomers;

public record GetCustomersQuery() : IRequest<List<Customer>>;
