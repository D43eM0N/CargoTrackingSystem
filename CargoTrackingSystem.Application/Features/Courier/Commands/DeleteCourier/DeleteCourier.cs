using MediatR;

namespace CargoTrackingSystem.Application.Features.Couriers.Commands.DeleteCourier;

public record DeleteCourierCommand(Guid Id) : IRequest<DeleteCourierResponse>;

public record DeleteCourierResponse(string Message);