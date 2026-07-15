using CargoTrackingSystem.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

//Defining aliases to prevent naming collisions between modules
using ShipmentEntity = CargoTrackingSystem.Domain.Entities.Shipment;


namespace CargoTrackingSystem.Application.Features.Shipment.Commands.CreateShipment;

public class CreateShipmentHandler : IRequestHandler<CreateShipment, CreateShipmentResponse>
{
    private readonly DbContext _context;
    private readonly IMemoryCache _cache;

    public CreateShipmentHandler(DbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<CreateShipmentResponse> Handle(CreateShipment request, CancellationToken cancellationToken)
    {
        var senderAddress = new Address
        {
            Id = Guid.NewGuid(),
            City = request.SenderCity,
            District = request.SenderDistrict,
            Street = request.SenderStreet,
            PostalCode = request.SenderPostalCode,
            FullAddress = request.SenderFullAddress
        };

        var receiverAddress = new Address
        {
            Id = Guid.NewGuid(),
            City = request.ReceiverCity,
            District = request.ReceiverDistrict,
            Street = request.ReceiverStreet,
            PostalCode = request.ReceiverPostalCode,
            FullAddress = request.ReceiverFullAddress
        };

        var shipment = new ShipmentEntity
        {
            Id = Guid.NewGuid(),
            TrackingNumber = request.TrackingNumber,
            SenderAddressId = senderAddress.Id,
            SenderAddress = senderAddress,
            ReceiverAddressId = receiverAddress.Id,
            ReceiverAddress = receiverAddress,
            Weight = request.Weight,
            Status = "Accepted",
            CreatedAt = DateTime.UtcNow
        };

        //Algorithm to choose most suitable Courier.
        var bestCourier = await _context.Set<Courier>()
            .Where(c => c.ResponsiblePostalCode == request.ReceiverPostalCode && c.IsActive)
            .OrderBy(c => c.ActiveLoad)
            .FirstOrDefaultAsync(cancellationToken);

        if (bestCourier != null)
        {
            bestCourier.ActiveLoad += 1;

            shipment.CourierId = bestCourier.Id;
            shipment.Courier = bestCourier;
        }

        var initialHistory = new ShipmentStatusHistory
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipment.Id,
            Status = shipment.Status,
            ChangedAt = DateTime.UtcNow
        };
        _context.Set<ShipmentStatusHistory>().Add(initialHistory);
        _context.Set<ShipmentEntity>().Add(shipment);

        await _context.SaveChangesAsync(cancellationToken);

        // Sends a cancellation signal to invalidate the token if the data changed
        if (_cache.TryGetValue("Shipments_Cache_Token", out CancellationTokenSource cts))
        {
            cts.Cancel(); 
        }

        String assignedCourierName;

        if (bestCourier != null)
        {
            assignedCourierName = $"{bestCourier.FirstName} {bestCourier.LastName}";
        }
        else
        {
            assignedCourierName = "No courier found for this area";
        }

        return new CreateShipmentResponse(
            Message: "Cargo Created Successfully!",
            TrackingNumber: shipment.TrackingNumber,
            AssignedCourier: assignedCourierName
        );
    }
}