using MediatR;

namespace CargoTrackingSystem.Application.Features.Couriers.Commands.ToggleCourierStatus;


public record ToggleCourierStatusCommand(Guid Id) : IRequest<ToggleCourierStatusResponse>;

public record ToggleCourierStatusResponse(string Message, bool IsActive, string CurrentStatus);