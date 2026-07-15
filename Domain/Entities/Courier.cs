using System;
using System.Text.Json.Serialization;

namespace CargoTrackingSystem.Domain.Entities
{
    public class Courier
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string ResponsiblePostalCode { get; set; } = string.Empty;
        public int ActiveLoad { get; set; }
    }
}