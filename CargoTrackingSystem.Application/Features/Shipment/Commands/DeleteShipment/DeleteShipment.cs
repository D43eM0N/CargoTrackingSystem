using MediatR;

namespace CargoTrackingSystem.Application.Features.Shipments.Commands.DeleteShipment;

public record DeleteShipmentCommand(Guid Id) : IRequest<DeleteShipmentResponse>;

public record DeleteShipmentResponse(string Message);
