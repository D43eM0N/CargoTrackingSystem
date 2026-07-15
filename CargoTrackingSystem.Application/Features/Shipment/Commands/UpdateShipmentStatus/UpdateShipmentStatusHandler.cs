using CargoTrackingSystem.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

//Defining aliases to prevent naming collisions between modules
using ShipmentEntity = CargoTrackingSystem.Domain.Entities.Shipment;

namespace CargoTrackingSystem.Application.Features.Shipments.Commands.UpdateShipmentStatus;

public class UpdateShipmentStatusHandler : IRequestHandler<UpdateShipmentStatusCommand, UpdateShipmentStatusResponse>
{
    private readonly DbContext _context;
    private readonly IMemoryCache _cache;

    public UpdateShipmentStatusHandler(DbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<UpdateShipmentStatusResponse> Handle(UpdateShipmentStatusCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _context.Set<ShipmentEntity>().FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (shipment == null)
        {
            throw new KeyNotFoundException("The shipment whose status needs to be updated could not found.");
        }

        var allowedStatuses = new List<string> { "Accepted", "PickedUp", "InTransit", "Out for delivery", "Delivered", "Cancelled" };

        if (!allowedStatuses.Contains(request.Status))
        {
            throw new ArgumentException($"Invalid Status. Possible Statuses: {string.Join(", ", allowedStatuses)}");
        }

        // Rule 1: Delivered Cargo's status can not change.
        if (shipment.Status == "Delivered" || shipment.Status == "Cancelled")
        {
            throw new InvalidOperationException("Delivered or Cancelled Cargo's state can not change!");
        }

        // Rule 2: Cannot switch status from "Accepted" to "Delivered" directy. First needs to goes out for delivery.
        if (shipment.Status == "Accepted" && request.Status != "PickedUp" && request.Status != "Cancelled")
        {
            throw new InvalidOperationException("The shipment not PickedUp Yet!");
        }

        // Rule 3: Picked Up Cargo can only switch be InTransit or Cancelled
        if (shipment.Status == "PickedUp" && request.Status != "InTransit" && request.Status != "Cancelled")
        {
            throw new InvalidOperationException($"A 'PickedUp' shipment can only transition to 'InTransit' or Cancelled.");
        }

        // Rule 4: InTransit cargo can only be OutForDelivery or Cancelled
        if (shipment.Status == "InTransit" && request.Status != "Out for delivery" && request.Status != "Cancelled")
        {
            throw new InvalidOperationException($"An 'InTransit' shipment can only transition to 'Out for delivery' or 'Cancelled'.");
        }

        // Rule 5: OutForDelivery can only be Delivered or Cancelled.
        if (shipment.Status == "Out for delivery" && request.Status != "Delivered" && request.Status != "Cancelled")
        {
            throw new InvalidOperationException($"An 'Out for delivery' shipment can only transition to 'Delivered' or 'Cancelled'.");
        }

        //Rule 6: If shipment Delivered or Cancelled decrease ActiveLoad by 1 for related Courier
        if ((request.Status == "Delivered" || request.Status == "Cancelled") && shipment.CourierId != null)
        {
            var courier = await _context.Set<Courier>().FirstOrDefaultAsync(c => c.Id == shipment.CourierId, cancellationToken);
            if (courier != null)
            {
                // To Prevent ActiveLoad be negative(Math.Max)
                courier.ActiveLoad = Math.Max(0, courier.ActiveLoad - 1);

                _context.Entry(courier).State = EntityState.Modified;
            }
        }

        var historyLog = new ShipmentStatusHistory
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipment.Id,
            Status = request.Status, 
            ChangedAt = DateTime.UtcNow 
        };

        _context.Set<ShipmentStatusHistory>().Add(historyLog);


        shipment.Status = request.Status;

        await _context.SaveChangesAsync(cancellationToken);

        // Sends a cancellation signal to invalidate the token if the data changed
        if (_cache.TryGetValue("Shipments_Cache_Token", out CancellationTokenSource cts))
        {
            cts.Cancel(); 
        }

        return new UpdateShipmentStatusResponse(
            Message: $"Cargo state '{request.Status}' updated Successfully.",
            CurrentStatus: shipment.Status
        );
    }
}