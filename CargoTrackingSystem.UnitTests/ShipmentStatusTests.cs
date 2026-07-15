using CargoTrackingSystem.Application.Features.Shipments.Commands.UpdateShipmentStatus;
using CargoTrackingSystem.Domain.Entities;
using CargoTrackingSystem.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;


public class ShipmentStatusTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task UpdateStatus_InvalidTransition()
    {
        // Negative Test senario
        // Arrange
        var context = GetInMemoryDbContext();

        var options = new MemoryCacheOptions();
        var mockCache = new MemoryCache(options);

        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            TrackingNumber = "TR9999",
            Status = "Delivered" //Delivered Cargo
        };
        context.Set<Shipment>().Add(shipment);
        await context.SaveChangesAsync();

        var handler = new UpdateShipmentStatusHandler(context, mockCache);
        var command = new UpdateShipmentStatusCommand(shipment.Id, "InTransit");

        // Act and Assert
        //InvalidOperationException or ArgumentException to be thrown during the invalid operation is Expected.
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateStatusAndLogHistory()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var options = new MemoryCacheOptions();
        var mockCache = new MemoryCache(options);

        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            TrackingNumber = "TR8888",
            Status = "InTransit"
        };
        context.Set<Shipment>().Add(shipment);
        await context.SaveChangesAsync();

        var handler = new UpdateShipmentStatusHandler(context, mockCache);
        var command = new UpdateShipmentStatusCommand(shipment.Id, "Out for delivery");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedShipment = await context.Set<Shipment>().FindAsync(shipment.Id);
        Assert.Equal("Out for delivery", updatedShipment.Status);

        // Verifying that sending logs through the History.
        var historyExists = await context.Set<ShipmentStatusHistory>()
            .AnyAsync(h => h.ShipmentId == shipment.Id && h.Status == "Out for delivery");
        Assert.True(historyExists);
    }

    [Fact]
    public async Task WhenTransitioningFromDeliveredToAnyStatus()
    {
        // Negative Test senario.
        // Arrange
        var context = GetInMemoryDbContext();

        var options = new MemoryCacheOptions();
        var mockCache = new MemoryCache(options);


        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            TrackingNumber = "TR1001",
            Status = "PickedUp" 
        };
        context.Set<Shipment>().Add(shipment);
        await context.SaveChangesAsync();

        var handler = new UpdateShipmentStatusHandler(context, mockCache);
        var command = new UpdateShipmentStatusCommand(shipment.Id, "Out for delivery"); // Invalid try must be InTransit first.

        // Act and Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task WhenTransitioningFromAcceptedToInvalidStatus()
    {   
        //Negative Test senario
        // Arrange
        var context = GetInMemoryDbContext();

        var options = new MemoryCacheOptions();
        var mockCache = new MemoryCache(options);


        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            TrackingNumber = "TR1002",
            Status = "Accepted" // Only can be pickedUp or Cancelled
        };
        context.Set<Shipment>().Add(shipment);
        await context.SaveChangesAsync();

        var handler = new UpdateShipmentStatusHandler(context, mockCache);
        var command = new UpdateShipmentStatusCommand(shipment.Id, "Out for delivery");

        // Act and Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task WhenTransitioningFromInTransitToOutForDelivery()
    {   
        // Arrange
        var context = GetInMemoryDbContext();

        var options = new MemoryCacheOptions();
        var mockCache = new MemoryCache(options);


        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            TrackingNumber = "TR1003",
            Status = "InTransit" //Only can be Out for delivery or Cancelled
        };
        context.Set<Shipment>().Add(shipment);
        await context.SaveChangesAsync();

        var handler = new UpdateShipmentStatusHandler(context, mockCache);
        var command = new UpdateShipmentStatusCommand(shipment.Id, "Out for delivery"); // Valid try

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedShipment = await context.Set<Shipment>().FindAsync(shipment.Id);
        Assert.Equal("Out for delivery", updatedShipment.Status);

        var historyExists = await context.Set<ShipmentStatusHistory>()
            .AnyAsync(h => h.ShipmentId == shipment.Id && h.Status == "Out for delivery");
        Assert.True(historyExists);
    }

    [Fact]
    public async Task WhenTransitioningFromInTransitToFailed()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var options = new MemoryCacheOptions();
        var mockCache = new MemoryCache(options);


        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            TrackingNumber = "TR1004",
            Status = "InTransit"
        };
        context.Set<Shipment>().Add(shipment);
        await context.SaveChangesAsync();

        var handler = new UpdateShipmentStatusHandler(context, mockCache);
        var command = new UpdateShipmentStatusCommand(shipment.Id, "Cancelled"); // Valid Try

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedShipment = await context.Set<Shipment>().FindAsync(shipment.Id);
        Assert.Equal("Cancelled", updatedShipment.Status);
    }
}
