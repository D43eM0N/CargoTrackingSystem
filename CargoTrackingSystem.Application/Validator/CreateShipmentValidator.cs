using CargoTrackingSystem.Application.Features.Shipment.Commands.CreateShipment;
using FluentValidation;

namespace CargoTrackingSystem.Application.Validators;

public class CreateShipmentValidator : AbstractValidator<CreateShipment>
{
    public CreateShipmentValidator()
    {
        RuleFor(s => s.TrackingNumber)
            .NotEmpty().WithMessage("Track number cannot null.")
            .MinimumLength(5).WithMessage("Trace Number must be atleast 5 character.");


        RuleFor(s => s.SenderCity)
            .NotEmpty().WithMessage("Sender's City cannot null.");

        RuleFor(s => s.ReceiverCity)
            .NotEmpty().WithMessage("Receiver's City cannot null.");


        RuleFor(s => s.SenderDistrict)
            .NotEmpty().WithMessage("Sender's District cannot null.");

        RuleFor(s => s.ReceiverDistrict)
            .NotEmpty().WithMessage("Receiver's District cannot null.");


        RuleFor(s => s.ReceiverPostalCode)
            .NotEmpty().WithMessage("Receiver's Postal Code cannot null.");


        RuleFor(s => s.SenderStreet)
            .NotEmpty().WithMessage("Sender's Street cannot null.");

        RuleFor(s => s.ReceiverStreet)
            .NotEmpty().WithMessage("Receiver's Street cannot null.");


        RuleFor(s => s.SenderFullAddress)
            .NotEmpty().WithMessage("Sender's Full Address cannot null.");

        RuleFor(s => s.ReceiverFullAddress)
            .NotEmpty().WithMessage("Receiver's Full Address cannot null.");


        RuleFor(s => s.Weight)
            .GreaterThan(0).WithMessage("Cargo Weight must be more than 0.")
            .LessThanOrEqualTo(150).WithMessage("Maximum 150KG is Accepted at Once.");
    }
}
