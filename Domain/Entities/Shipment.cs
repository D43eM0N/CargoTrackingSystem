using System;

namespace CargoTrackingSystem.Domain.Entities
{
    public class Shipment
    {
        public Guid Id { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;

        public Guid SenderAddressId { get; set; }
        public Guid ReceiverAddressId { get; set; }
        public Address SenderAddress { get; set; } = null!;
        public Address ReceiverAddress { get; set; } = null!;

        public string Status { get; set; } = "None";
        public double Weight { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? CourierId { get; set; } 
        public Courier? Courier { get; set; }

        public List<ShipmentStatusHistory> StatusHistories { get; set; } = new();

    }
}