using CargoTrackingSystem.Application.Features.Couriers.Commands.CreateCourier;
using CargoTrackingSystem.Application.Features.Couriers.Commands.DeleteCourier;
using CargoTrackingSystem.Application.Features.Couriers.Commands.ToggleCourierStatus;
using CargoTrackingSystem.Application.Features.Couriers.Commands.UpdateCourier;
using CargoTrackingSystem.Application.Features.Couriers.Queries.GetCouriers;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrackingSystem.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouriersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateCourierCommand> _createValidator;
    private readonly IValidator<UpdateCourierCommand> _updateValidator;

    public CouriersController(
        IMediator mediator,
        IValidator<CreateCourierCommand> createValidator,
        IValidator<UpdateCourierCommand> updateValidator)
    {
        _mediator = mediator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    //Read
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var couriers = await _mediator.Send(new GetCouriersQuery());
        return Ok(couriers);
    }

    // Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourierCommand command)
    {
        var validationResult = await _createValidator.ValidateAsync(command);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage }));
        }

        var response = await _mediator.Send(command);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var response = await _mediator.Send(new DeleteCourierCommand(id));
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
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourierCommand command)
    {
        var validationResult = await _updateValidator.ValidateAsync(command);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage }));
        }

        var mainCommand = new UpdateCourierCommand(
            id,
            command.FirstName,
            command.LastName,
            command.VehicleType,
            command.ResponsiblePostalCode
        );

        try
        {
            var response = await _mediator.Send(mainCommand);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    // Toggle Status
    [HttpPut("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        try
        {
            var response = await _mediator.Send(new ToggleCourierStatusCommand(id));
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}