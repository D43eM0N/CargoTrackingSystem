using CargoTrackingSystem.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

//Defining aliases to prevent naming collisions between modules
using CourierEntity = CargoTrackingSystem.Domain.Entities.Courier;
using ShipmentEntity = CargoTrackingSystem.Domain.Entities.Shipment;

namespace CargoTrackingSystem.Application.Features.Couriers.Commands.DeleteCourier;

public class DeleteCourierHandler : IRequestHandler<DeleteCourierCommand, DeleteCourierResponse>
{
    private readonly DbContext _context;
    private readonly IMemoryCache _cache;

    public DeleteCourierHandler(DbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<DeleteCourierResponse> Handle(DeleteCourierCommand request, CancellationToken cancellationToken)
    {
        var courier = await _context.Set<CourierEntity>().FindAsync(new object[] { request.Id }, cancellationToken);

        if (courier == null)
        {
            throw new KeyNotFoundException("Courier that wanted to Delete not found!");
        }

        //Not allows Deletion if isActive is True
        if (courier.IsActive)
        {
            throw new InvalidOperationException("Active couriers cannot be deleted! First turn IsActive to False using toggle-status.");
        }

        //Check if Courier has ongoing Cargo to deliver
        var hasActiveShipments = await _context.Set<ShipmentEntity>()
            .AnyAsync(s => s.CourierId == request.Id && s.Status != "Delivered", cancellationToken);

        if (hasActiveShipments)
        {
            throw new InvalidOperationException("This Courier currently has active ongoing cargos to deliver. Cannot be deleted!");
        }

        // Remove Relation and make Courier Name and Id null in shipment table
        var deliveredShipments = await _context.Set<ShipmentEntity>()
            .Where(s => s.CourierId == request.Id && s.Status == "Delivered")
            .ToListAsync(cancellationToken);

        foreach (var shipment in deliveredShipments)
        {
            shipment.CourierId = null;
            shipment.Courier = null;
        }

        _context.Set<CourierEntity>().Remove(courier);
        await _context.SaveChangesAsync(cancellationToken);

        // Sends a cancellation signal to invalidate the token if the data changed
        if (_cache.TryGetValue("Couriers_Cache_Token", out CancellationTokenSource courierCts))
        {
            courierCts.Cancel();
        }

        return new DeleteCourierResponse($"{courier.FirstName} {courier.LastName} named Courier deleted successfully.");
    }
}