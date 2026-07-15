using System;

namespace CargoTrackingSystem.Domain.Entities
{
    public class Address
    {
        public Guid Id { get; set; }
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
    }
}