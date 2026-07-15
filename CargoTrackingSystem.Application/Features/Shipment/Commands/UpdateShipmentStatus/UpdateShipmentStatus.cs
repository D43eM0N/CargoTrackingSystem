using MediatR;
using System.Text.Json.Serialization;

namespace CargoTrackingSystem.Application.Features.Shipments.Commands.UpdateShipmentStatus;

public record UpdateShipmentStatusCommand(
    [property: JsonIgnore]
    Guid Id, 
    string Status) : IRequest<UpdateShipmentStatusResponse>;

public record UpdateShipmentStatusResponse(string Message, string CurrentStatus);
