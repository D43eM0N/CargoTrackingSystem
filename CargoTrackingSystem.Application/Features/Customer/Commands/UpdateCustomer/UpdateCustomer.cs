using MediatR;
using System.Text.Json.Serialization;

namespace CargoTrackingSystem.Application.Features.Customers.Commands.UpdateCustomer;


//Tells System that it is a MediatR Request(IRequest) and it's execution will return Response.
public record UpdateCustomerCommand : IRequest<UpdateCustomerResponse>
{
    [property: JsonIgnore]
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}

//The Resonse Value.
public record UpdateCustomerResponse(string Message);
