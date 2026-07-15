using MediatR;
using System.Text.Json.Serialization;

namespace CargoTrackingSystem.Application.Features.Couriers.Commands.UpdateCourier;

public record UpdateCourierCommand(

    [property: JsonIgnore]
    Guid Id,
    string? FirstName = null,
    string? LastName = null,
    string? VehicleType = null,
    string? ResponsiblePostalCode = null
) : IRequest<UpdateCourierResponse>;

public record UpdateCourierResponse(string Message);