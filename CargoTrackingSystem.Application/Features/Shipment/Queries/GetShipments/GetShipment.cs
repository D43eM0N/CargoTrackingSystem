using MediatR;

namespace CargoTrackingSystem.Application.Features.Shipment.Queries.GetShipments;

public record GetShipmentsQuery(
    // Page settings can be changed from here to make it default.
    int Page = 1,
    int PageSize = 3,

    string? Status = null,
    Guid? CourierId = null,
    DateTime? CreatedAt = null,
    string? TrackingNumber = null

) : IRequest<PagedShipmentsResponse>;

public record PagedShipmentsResponse(
    // Shipment Objects
    object Items,
    int CurrentPage,
    int TotalPages,
    int TotalCargo
);