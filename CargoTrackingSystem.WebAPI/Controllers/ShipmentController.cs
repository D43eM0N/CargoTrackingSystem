using CargoTrackingSystem.Application.Features.Shipment.Commands.CreateShipment;
using CargoTrackingSystem.Application.Features.Shipment.Queries.GetShipments;
using CargoTrackingSystem.Application.Features.Shipments.Commands.DeleteShipment;
using CargoTrackingSystem.Application.Features.Shipments.Commands.UpdateShipment;
using CargoTrackingSystem.Application.Features.Shipments.Commands.UpdateShipmentStatus;
using CargoTrackingSystem.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CargoTrackingSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IValidator<CreateShipment> _createValidator;
        private readonly IValidator<UpdateShipment> _updateValidator;
        private readonly IMediator _mediator;

        public ShipmentsController(AppDbContext context, IValidator<CreateShipment> createValidator, IValidator<UpdateShipment> updateValidator, IMediator mediator)
        {
            _context = context;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _mediator = mediator;
        }

        // Read
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetShipmentsQuery query)
        {   
            var shipments = await _mediator.Send(query);
            return Ok(shipments);
        }
        // Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateShipment command)
        {
            var validationResult = await _createValidator.ValidateAsync(command);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage }));
            }

            var Object = new CreateShipment(
            command.TrackingNumber,
            command.SenderCity, command.SenderDistrict, command.SenderStreet, command.SenderPostalCode, command.SenderFullAddress,
            command.ReceiverCity, command.ReceiverDistrict, command.ReceiverStreet, command.ReceiverPostalCode, command.ReceiverFullAddress,
            command.Weight
        );
            var result = await _mediator.Send(Object);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var response = await _mediator.Send(new DeleteShipmentCommand(id));
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        // Update
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateShipment command)
        {
            var validationResult = await _updateValidator.ValidateAsync(command);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage }));
            }

            var Object = new UpdateShipment(
                id, command.TrackingNumber, command.Weight,
                command.SenderCity, command.SenderDistrict, command.SenderStreet, command.SenderPostalCode, command.SenderFullAddress,
                command.ReceiverCity, command.ReceiverDistrict, command.ReceiverStreet, command.ReceiverPostalCode, command.ReceiverFullAddress
            );

            try
            {
                var response = await _mediator.Send(Object);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateShipmentStatusCommand command)
        {
            try
            {
                var response = await _mediator.Send(new UpdateShipmentStatusCommand(id, command.Status));
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
