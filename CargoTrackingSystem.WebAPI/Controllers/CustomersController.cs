using CargoTrackingSystem.Application.Features.Customers.Commands.CreateCustomer;
using CargoTrackingSystem.Application.Features.Customers.Commands.DeleteCustomer;
using CargoTrackingSystem.Application.Features.Customers.Commands.UpdateCustomer;
using CargoTrackingSystem.Application.Features.Customers.Queries.GetCustomers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrackingSystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Read
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var customers = await _mediator.Send(new GetCustomersQuery());
        return Ok(customers);
    }

    // Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var response = await _mediator.Send(new DeleteCustomerCommand(id));
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    // UPDATE
    [HttpPut("{customerId}")]
    public async Task<IActionResult> Update([FromRoute] Guid customerId, [FromBody] UpdateCustomerCommand command)
    {
        // Manually inject the ID from the URL to prevent a conflict between the route and the body.
        command.Id = customerId;

        try
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}