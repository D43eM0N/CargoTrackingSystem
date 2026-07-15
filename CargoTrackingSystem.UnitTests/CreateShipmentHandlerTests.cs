using CargoTrackingSystem.Application.Features.Shipment.Commands.CreateShipment;
using CargoTrackingSystem.Domain.Entities;
using CargoTrackingSystem.Infrastructure; 
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory; 

public class CreateShipmentHandlerTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }



    [Fact]
    public async Task AssignCourierAndIncreaseActiveLoad()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var cacheOptions = new MemoryCacheOptions();
        var mockCache = new MemoryCache(cacheOptions);

        var courier = new Courier
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            ResponsiblePostalCode = "34000",
            IsActive = true,
            ActiveLoad = 2
        };
        context.Set<Courier>().Add(courier);
        await context.SaveChangesAsync();

        var handler = new CreateShipmentHandler(context, mockCache);
        var command = new CreateShipment("TR123456", "Ist", "Kadikoy", "St", "34000", "Full Address", "Ist", "Uskudar", "St", "34000", "Full Address", 10.5);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result.TrackingNumber);
        Assert.Contains("John Doe", result.AssignedCourier);

        // Verify that Active Load is increased by 1 in db.
        var updatedCourier = await context.Set<Courier>().FindAsync(courier.Id);
        Assert.Equal(3, updatedCourier.ActiveLoad);
    }

    [Fact]
    public async Task AssignCourierWithMinimumActiveLoad()
    {
        // Arrange 
        var context = GetInMemoryDbContext();

        var cacheOptions = new MemoryCacheOptions();
        var mockCache = new MemoryCache(cacheOptions);

        var busyCourier = new Courier { Id = Guid.NewGuid(), FirstName = "busy", LastName = "Courier", ResponsiblePostalCode = "34000", IsActive = true, ActiveLoad = 5 };
        var freeCourier = new Courier { Id = Guid.NewGuid(), FirstName = "Available", LastName = "Courier", ResponsiblePostalCode = "34000", IsActive = true, ActiveLoad = 1 };

        context.Set<Courier>().AddRange(busyCourier, freeCourier);
        await context.SaveChangesAsync();

        var handler = new CreateShipmentHandler(context, mockCache);
        var command = new CreateShipment("TR123456", "Ist", "Kadikoy", "St", "34000", "Full Address", "Ist", "Uskudar", "St", "34000", "Full Address", 10.5);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Contains("Available Courier", result.AssignedCourier);
    }

    [Fact]
    public async Task WhenNoCourierInPostalCode_ReturnNoCourierMessage()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var cacheOptions = new MemoryCacheOptions();
        var mockCache = new MemoryCache(cacheOptions);

        var handler = new CreateShipmentHandler(context, mockCache);
        var command = new CreateShipment("TR123456", "Ist", "Kadikoy", "St", "34000", "Full Address", "Ist", "Uskudar", "St", "99999", "Full Address", 10.5);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("No courier found for this area", result.AssignedCourier);
    }

    [Fact]
    public async Task WhenCourierIsInactive_NotAssignCourier()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var cacheOptions = new MemoryCacheOptions();
        var mockCache = new MemoryCache(cacheOptions);

        var inactiveCourier = new Courier { Id = Guid.NewGuid(), FirstName = "Test", LastName = "Courier", ResponsiblePostalCode = "34000", IsActive = false, ActiveLoad = 0 };
        context.Set<Courier>().Add(inactiveCourier);
        await context.SaveChangesAsync();

        var handler = new CreateShipmentHandler(context, mockCache);
        var command = new CreateShipment("TR123456", "Ist", "Kadikoy", "St", "34000", "Full Address", "Ist", "Uskudar", "St", "34000", "Full Address", 10.5);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("No courier found for this area", result.AssignedCourier);
    }

    [Fact]
    public async Task CreateInitialStatusAsAccepted()
    {
        // Arrange
        var context = GetInMemoryDbContext();

        var cacheOptions = new MemoryCacheOptions();
        var mockCache = new MemoryCache(cacheOptions);

        var handler = new CreateShipmentHandler(context, mockCache);
        var command = new CreateShipment("TR123456", "Ist", "Kadikoy", "St", "34000", "Full Address", "Ist", "Uskudar", "St", "34000", "Full Address", 10.5);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var history = await context.Set<ShipmentStatusHistory>().FirstOrDefaultAsync();
        Assert.NotNull(history);
        Assert.Equal("Accepted", history.Status);
    }
}