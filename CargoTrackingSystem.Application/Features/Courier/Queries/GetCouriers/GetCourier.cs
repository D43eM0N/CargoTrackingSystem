using MediatR;
using CargoTrackingSystem.Domain.Entities;

namespace CargoTrackingSystem.Application.Features.Couriers.Queries.GetCouriers;

public record GetCouriersQuery() : IRequest<List<Courier>>;