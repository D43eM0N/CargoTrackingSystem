using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

//Defining aliases to prevent naming collisions between modules
using CourierEntity = CargoTrackingSystem.Domain.Entities.Courier;

namespace CargoTrackingSystem.Application.Features.Couriers.Commands.UpdateCourier;

public class UpdateCourierHandler : IRequestHandler<UpdateCourierCommand, UpdateCourierResponse>
{
    private readonly DbContext _context;
    private readonly IMemoryCache _cache;

    public UpdateCourierHandler(DbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<UpdateCourierResponse> Handle(UpdateCourierCommand request, CancellationToken cancellationToken)
    {
        var courier = await _context.Set<CourierEntity>()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (courier == null)
        {
            throw new KeyNotFoundException("The courier to be updated could not found.");
        }

        //Not Specified Values will be null by default and will skipped and remain unchanged.
        if (!string.IsNullOrEmpty(request.FirstName))
            courier.FirstName = request.FirstName;

        if (!string.IsNullOrEmpty(request.LastName))
            courier.LastName = request.LastName;

        if (!string.IsNullOrEmpty(request.VehicleType))
            courier.VehicleType = request.VehicleType;

        if (!string.IsNullOrEmpty(request.ResponsiblePostalCode))
            courier.ResponsiblePostalCode = request.ResponsiblePostalCode;

        await _context.SaveChangesAsync(cancellationToken);

        // Sends a cancellation signal to invalidate the token if the data changed
        if (_cache.TryGetValue("Couriers_Cache_Token", out CancellationTokenSource courierCts))
        {
            courierCts.Cancel();
        }

        return new UpdateCourierResponse("Courier information updated successfully!");
    }
}