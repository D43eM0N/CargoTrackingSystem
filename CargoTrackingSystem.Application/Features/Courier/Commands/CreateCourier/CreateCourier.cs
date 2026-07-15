using MediatR;

namespace CargoTrackingSystem.Application.Features.Couriers.Commands.CreateCourier;

public record CreateCourierCommand(
    string FirstName,
    string LastName,
    string VehicleType,
    bool IsActive,
    string ResponsiblePostalCode,
    int ActiveLoad
) : IRequest<CreateCourierResponse>;


public record CreateCourierResponse(string Message, Guid CourierId);
