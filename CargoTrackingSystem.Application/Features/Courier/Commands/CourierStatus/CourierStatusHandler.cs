using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

//Defining aliases to prevent naming collisions between modules
using CourierEntity = CargoTrackingSystem.Domain.Entities.Courier;

namespace CargoTrackingSystem.Application.Features.Couriers.Commands.ToggleCourierStatus;

public class ToggleCourierStatusHandler : IRequestHandler<ToggleCourierStatusCommand, ToggleCourierStatusResponse>
{
    private readonly DbContext _context;
    private readonly IMemoryCache _cache;

    public ToggleCourierStatusHandler(DbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ToggleCourierStatusResponse> Handle(ToggleCourierStatusCommand request, CancellationToken cancellationToken)
    {
        var courier = await _context.Set<CourierEntity>().FindAsync(new object[] { request.Id }, cancellationToken);

        if (courier == null)
        {
            throw new KeyNotFoundException("Courier Not Found!");
        }

        courier.IsActive = !courier.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        // Sends a cancellation signal to invalidate the token if the data changed
        if (_cache.TryGetValue("Couriers_Cache_Token", out CancellationTokenSource courierCts))
        {
            courierCts.Cancel();
        }

        string courierStatus = courier.IsActive ? "Active" : "Passive";

        return new ToggleCourierStatusResponse(
            Message: $"{courier.FirstName} {courier.LastName} named Courier Condition Changed.",
            IsActive: courier.IsActive,
            CurrentStatus: $"Courier is now, {courierStatus}"
        );
    }
}