using CargoTrackingSystem.Application.Features.Couriers.Commands.CreateCourier;
using CargoTrackingSystem.Application.Features.Customers.Commands.CreateCustomer;
using CargoTrackingSystem.Application.Features.Shipment.Commands.CreateShipment;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json;

namespace CargoTrackingSystem.WebAPI.Infrastructure.OpenApi;

public class ExampleSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        //Shipment Example
        if (context.JsonTypeInfo.Type == typeof(CreateShipment))
        {
            var requestExample = new Dictionary<string, object>
            {
                ["trackingNumber"] = "TR-2026-9988X",
                ["senderCity"] = "Istanbul",
                ["senderDistrict"] = "Kadikoy",
                ["senderStreet"] = "Moda Cad. No:12",
                ["senderPostalCode"] = "34710",
                ["senderFullAddress"] = "Caferağa Mah. Moda Cad. No:12 D:4, Kadıköy/İstanbul",
                ["receiverCity"] = "Ankara",
                ["receiverDistrict"] = "Cankaya",
                ["receiverStreet"] = "Ataturk Bulvari No:101",
                ["receiverPostalCode"] = "06680",
                ["receiverFullAddress"] = "Kavaklıdere Mah. Atatürk Bulvarı No:101, Çankaya/Ankara",
                ["weight"] = 4.75
            };
            schema.Example = JsonSerializer.SerializeToNode(requestExample);
        }

        if (context.JsonTypeInfo.Type == typeof(CreateShipmentResponse))
        {
            var responseExample = new Dictionary<string, object>
            {
                ["message"] = "Cargo Created Successfully!",
                ["trackingNumber"] = "TR-2026-9988X",
                ["assignedCourier"] = "John Doe"
            };
            schema.Example = JsonSerializer.SerializeToNode(responseExample);
        }

        //Courier Example
        if (context.JsonTypeInfo.Type == typeof(CreateCourierCommand))
        {
            var requestExample = new Dictionary<string, object>
            {
                ["firstName"] = "John",
                ["lastName"] = "Doe",
                ["vehicleType"] = "Motorcycle",
                ["isActive"] = true,
                ["responsiblePostalCode"] = "06680",
                ["activeLoad"] = 0
            };
            schema.Example = JsonSerializer.SerializeToNode(requestExample);
        }

        if (context.JsonTypeInfo.Type == typeof(CreateCourierResponse))
        {
            var responseExample = new Dictionary<string, object>
            {
                ["message"] = "Courier Created Successfully!",
                ["courierId"] = Guid.NewGuid()
            };
            schema.Example = JsonSerializer.SerializeToNode(responseExample);
        }

       
        //Customer Example
        if (context.JsonTypeInfo.Type == typeof(CreateCustomerCommand))
        {
            var requestExample = new Dictionary<string, object>
            {
                ["fullName"] = "John Doe",
                ["email"] = "john.doe@example.com",
                ["phoneNumber"] = "+905000000000"
            };
            schema.Example = JsonSerializer.SerializeToNode(requestExample);
        }

        if (context.JsonTypeInfo.Type == typeof(CreateCustomerResponse))
        {
            var responseExample = new Dictionary<string, object>
            {
                ["message"] = "Customer Created Successfully!",
                ["customerId"] = Guid.NewGuid()
            };
            schema.Example = JsonSerializer.SerializeToNode(responseExample);
        }

        return Task.CompletedTask;
    }
}