using MediatR;

namespace CargoTrackingSystem.Application.Features.Shipment.Commands.CreateShipment;

public record CreateShipment(
    string TrackingNumber,
    string SenderCity,
    string SenderDistrict,
    string SenderStreet,
    string SenderPostalCode,
    string SenderFullAddress,
    string ReceiverCity,
    string ReceiverDistrict,
    string ReceiverStreet,
    string ReceiverPostalCode,
    string ReceiverFullAddress,
    double Weight
) : IRequest<CreateShipmentResponse>;

public record CreateShipmentResponse(
    string Message,
    string TrackingNumber,
    string AssignedCourier
);