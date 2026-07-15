using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using CargoTrackingSystem.Domain.Entities;

//Defining aliases to prevent naming collisions between modules
using ShipmentEntity = CargoTrackingSystem.Domain.Entities.Shipment;

namespace CargoTrackingSystem.Application.Features.Shipments.Commands.UpdateShipment;

public class UpdateShipmentHandler : IRequestHandler<UpdateShipment, UpdateShipmentResponse>
{
    private readonly DbContext _context;
    private readonly IMemoryCache _cache;
    public UpdateShipmentHandler(DbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<UpdateShipmentResponse> Handle(UpdateShipment request, CancellationToken cancellationToken)
    {
        var shipment = await _context.Set<ShipmentEntity>()
            .Include(s => s.SenderAddress)
            .Include(s => s.ReceiverAddress)
            .Include(s => s.Courier) 
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (shipment == null)
        {
            throw new KeyNotFoundException("The shipment to be updated could not found.");
        }

        if (!string.IsNullOrEmpty(request.TrackingNumber))
            shipment.TrackingNumber = request.TrackingNumber;

        // Weight rule
        if (request.Weight.HasValue && request.Weight.Value > 0)
        {
            shipment.Weight = request.Weight.Value;
        }

        //Not Specified Values will be null by default and will skipped and remain unchanged
        if (shipment.SenderAddress != null)
        {
            if (!string.IsNullOrEmpty(request.SenderCity)) shipment.SenderAddress.City = request.SenderCity;
            if (!string.IsNullOrEmpty(request.SenderDistrict)) shipment.SenderAddress.District = request.SenderDistrict;
            if (!string.IsNullOrEmpty(request.SenderStreet)) shipment.SenderAddress.Street = request.SenderStreet;
            if (!string.IsNullOrEmpty(request.SenderPostalCode)) shipment.SenderAddress.PostalCode = request.SenderPostalCode;
            if (!string.IsNullOrEmpty(request.SenderFullAddress)) shipment.SenderAddress.FullAddress = request.SenderFullAddress;
        }

        var oldPostalCode = shipment.ReceiverAddress?.PostalCode;

        if (shipment.ReceiverAddress != null)
        {
            if (!string.IsNullOrEmpty(request.ReceiverCity)) shipment.ReceiverAddress.City = request.ReceiverCity;
            if (!string.IsNullOrEmpty(request.ReceiverDistrict)) shipment.ReceiverAddress.District = request.ReceiverDistrict;
            if (!string.IsNullOrEmpty(request.ReceiverStreet)) shipment.ReceiverAddress.Street = request.ReceiverStreet;
            if (!string.IsNullOrEmpty(request.ReceiverPostalCode)) shipment.ReceiverAddress.PostalCode = request.ReceiverPostalCode;
            if (!string.IsNullOrEmpty(request.ReceiverFullAddress)) shipment.ReceiverAddress.FullAddress = request.ReceiverFullAddress;
        }

        /*
        If the reciever's postal code has changed OR if there is currently no courier assigned to the shipment,
        recalculate the courier assignment.
        */
        var currentPostalCode = shipment.ReceiverAddress?.PostalCode;
        if (oldPostalCode != currentPostalCode || shipment.CourierId == null)
        {
            var bestCourier = await _context.Set<Courier>()
                .Where(c => c.ResponsiblePostalCode == currentPostalCode && c.IsActive)
                .OrderBy(c => c.ActiveLoad)
                .FirstOrDefaultAsync(cancellationToken);

            if (shipment.Courier != null && (bestCourier == null || bestCourier.Id != shipment.CourierId))
            {
                shipment.Courier.ActiveLoad = Math.Max(0, shipment.Courier.ActiveLoad - 1);
            }

            if (bestCourier != null)
            {

                if (shipment.CourierId != bestCourier.Id)
                {
                    bestCourier.ActiveLoad += 1;
                }

                shipment.CourierId = bestCourier.Id;
                shipment.Courier = bestCourier;
            }
            else
            {
                shipment.CourierId = null;
                shipment.Courier = null;
            }
        }

        // Sends a cancellation signal to invalidate the token if the data changed
        await _context.SaveChangesAsync(cancellationToken);

        if (_cache.TryGetValue("Shipments_Cache_Token", out CancellationTokenSource cts))
        {
            cts.Cancel(); 
        }

        return new UpdateShipmentResponse("The cargo Information Updated successfully!");
    }
}