using CargoTrackingSystem.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

//Defining aliases to prevent naming collisions between modules
using ShipmentEntity = CargoTrackingSystem.Domain.Entities.Shipment;

namespace CargoTrackingSystem.Application.Features.Shipments.Commands.DeleteShipment;


public class DeleteShipmentHandler : IRequestHandler<DeleteShipmentCommand, DeleteShipmentResponse>
{
    private readonly DbContext _context;
    private readonly IMemoryCache _cache;
    public DeleteShipmentHandler(DbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<DeleteShipmentResponse> Handle(DeleteShipmentCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _context.Set<ShipmentEntity>()
            .Include(s => s.SenderAddress)
            .Include(s => s.ReceiverAddress)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (shipment == null)
        {
            throw new KeyNotFoundException("The shipment to be deleted not found.");
        }

        if (shipment.CourierId != null && shipment.Status != "Delivered" && shipment.Status != "Cancelled")
        {
            var courier = await _context.Set<Courier>().FirstOrDefaultAsync(c => c.Id == shipment.CourierId, cancellationToken);
            if (courier != null)
            {
                // To Prevent ActiveLoad be negative(Math.Max)
                courier.ActiveLoad = Math.Max(0, courier.ActiveLoad - 1);

                _context.Entry(courier).State = EntityState.Modified;
            }
        }

        var senderAddress = shipment.SenderAddress;
        var receiverAddress = shipment.ReceiverAddress;

        _context.Set<ShipmentEntity>().Remove(shipment);

        if (senderAddress != null) _context.Set<Address>().Remove(senderAddress);
        if (receiverAddress != null) _context.Set<Address>().Remove(receiverAddress);

        await _context.SaveChangesAsync(cancellationToken);

        // Sends a cancellation signal to invalidate the token if the data changed
        if (_cache.TryGetValue("Shipments_Cache_Token", out CancellationTokenSource cts))
        {
            cts.Cancel(); 
        }

        return new DeleteShipmentResponse("Cargo and Related Address Deleted Successfully!");
    }
}
