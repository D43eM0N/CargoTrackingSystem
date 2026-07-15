using MediatR;
using System.Text.Json.Serialization;

namespace CargoTrackingSystem.Application.Features.Shipments.Commands.UpdateShipment;


public record UpdateShipment(

    [property: JsonIgnore]
    Guid Id,
    string? TrackingNumber,
    double? Weight,
    string? SenderCity,
    string? SenderDistrict,
    string? SenderStreet,
    string? SenderPostalCode,
    string? SenderFullAddress,
    string? ReceiverCity,
    string? ReceiverDistrict,
    string? ReceiverStreet,
    string? ReceiverPostalCode,
    string? ReceiverFullAddress
) : IRequest<UpdateShipmentResponse>;

public record UpdateShipmentResponse(
    string Message
);
