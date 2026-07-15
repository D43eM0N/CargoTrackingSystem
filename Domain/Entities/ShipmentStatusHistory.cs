using System.Text.Json.Serialization;

namespace CargoTrackingSystem.Domain.Entities
{
    public class ShipmentStatusHistory
    {   
        public Guid Id { get; set; }

        [JsonIgnore]
        public Guid ShipmentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }

        [JsonIgnore]
        public Shipment? Shipment { get; set; }

    }
}
