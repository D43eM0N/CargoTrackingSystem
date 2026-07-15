using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using CargoTrackingSystem.Domain.Entities; 

namespace CargoTrackingSystem.Infrastructure
{
    public class CargoStatusWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CargoStatusWorker> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(1);

        public CargoStatusWorker(IServiceProvider serviceProvider, ILogger<CargoStatusWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cargo status update Service Started Successfully!");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Checking Cargo statuses in background...");

                    //BackgroundService is singleton and AppDbContext is scope so they won't work together
                    //To fix: We're creating an isolated memory for scope inside of Singleton to run AppDbContext
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        // InTransit to Out for delivery
                        var inTransitShipments = await context.Shipments
                            .Where(s => s.Status == "InTransit")
                            .ToListAsync(stoppingToken);

                        if (inTransitShipments.Any())
                        {
                            foreach (var shipment in inTransitShipments)
                            {
                                shipment.Status = "Out for delivery";
                                _logger.LogInformation($"Shipment ID: {shipment.Id} status updated to 'Out for delivery' automatically.");

                                var historyLog = new ShipmentStatusHistory
                                {
                                    Id = Guid.NewGuid(),
                                    ShipmentId = shipment.Id,
                                    Status = "Out for delivery",
                                    ChangedAt = DateTime.UtcNow
                                };

                                context.Set<ShipmentStatusHistory>().Add(historyLog);
                            }
                        }

                        // Out for delivery to Delivered
                        var outForDeliveryShipments = await context.Shipments
                            .Where(s => s.Status == "Out for delivery")
                            .ToListAsync(stoppingToken);

                        if (outForDeliveryShipments.Any())
                        {
                            foreach (var shipment in outForDeliveryShipments)
                            {
                                // If status in InTransit
                                var entry = context.Entry(shipment);
                                if (entry.State == EntityState.Modified && entry.Property(x => x.Status).OriginalValue == "InTransit")
                                {
                                    continue; // it must be Out for delivery first
                                }

                                shipment.Status = "Delivered";
                                _logger.LogInformation($"Shipment ID: {shipment.Id} status updated to 'Delivered' automatically.");

                                var historyLog = new ShipmentStatusHistory
                                {
                                    Id = Guid.NewGuid(),
                                    ShipmentId = shipment.Id,
                                    Status = "Delivered",
                                    ChangedAt = DateTime.UtcNow
                                };

                                context.Set<ShipmentStatusHistory>().Add(historyLog);
                            }
                        }

                        if (context.ChangeTracker.HasChanges())
                        {
                            await context.SaveChangesAsync(stoppingToken);
                        }
                        else
                        {
                            _logger.LogInformation("No shipments found that need status updates.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred in the background while shipment statuses were automatically updated.");
                }

                await Task.Delay(_period, stoppingToken);
            }
        }
    }
}