using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

//Defining aliases to prevent naming collisions between modules
using CourierEntity = CargoTrackingSystem.Domain.Entities.Courier;
using ShipmentEntity = CargoTrackingSystem.Domain.Entities.Shipment;

namespace CargoTrackingSystem.Application.Features.Couriers.Commands.CreateCourier;

public class CreateCourierHandler : IRequestHandler<CreateCourierCommand, CreateCourierResponse>
{
    private readonly DbContext _context;
    private readonly IMemoryCache _cache;

    public CreateCourierHandler(DbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<CreateCourierResponse> Handle(CreateCourierCommand request, CancellationToken cancellationToken)
    {
        var courier = new CourierEntity
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            VehicleType = request.VehicleType,
            IsActive = request.IsActive,
            ResponsiblePostalCode = request.ResponsiblePostalCode,
            ActiveLoad = request.ActiveLoad
        };
        _context.Set<CourierEntity>().Add(courier);

        // Auto assign new courier to the unassigned shipments if PostalCode matches
        if (courier.IsActive && !string.IsNullOrEmpty(courier.ResponsiblePostalCode))
        {
            var unassignedShipments = await _context.Set<ShipmentEntity>()
                .Include(s => s.ReceiverAddress) 
                .Where(s => s.ReceiverAddress.PostalCode == courier.ResponsiblePostalCode && s.CourierId == null)
                .ToListAsync(cancellationToken);

            if (unassignedShipments.Any())
            {
                foreach (var shipment in unassignedShipments)
                {
                    shipment.CourierId = courier.Id;
                    shipment.Courier = courier;
                }

                courier.ActiveLoad += unassignedShipments.Count;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Sends a cancellation signal to invalidate the token if the data changed
        if (_cache.TryGetValue("Couriers_Cache_Token", out CancellationTokenSource courierCts))
        {
            courierCts.Cancel();
        }

        return new CreateCourierResponse("Courier Added to System Successfully!", courier.Id);
    }
}