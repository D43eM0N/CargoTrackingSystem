using CargoTrackingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CargoTrackingSystem.Infrastructure
{
    // Inheriting capabilities of DbContext to AppDbContext.
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<Address>  Addresses { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Courier> Couriers { get; set; }
        public DbSet<ShipmentStatusHistory> ShipmentStatusHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Shipment>()
            .HasOne(s => s.SenderAddress) 
            .WithMany()
            .HasForeignKey(s => s.SenderAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Shipment>()
            .HasOne(s => s.ReceiverAddress)
            .WithMany()
            .HasForeignKey(s => s.ReceiverAddressId)
            //Restriction that not allows deletion if there is current cargo related that address.
            .OnDelete(DeleteBehavior.Restrict);
        }
    }
}