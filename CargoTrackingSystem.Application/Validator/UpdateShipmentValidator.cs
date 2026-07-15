using FluentValidation;
using CargoTrackingSystem.Application.Features.Shipments.Commands.UpdateShipment;

namespace CargoTrackingSystem.Application.Validators;

public class UpdateShipmentValidator : AbstractValidator<UpdateShipment>
{
    public UpdateShipmentValidator()
    {
        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Cargo Weight must be more than 0.")
            .LessThanOrEqualTo(150).WithMessage("Maximum 150KG is Accepted at Once.")
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.TrackingNumber)
            .MinimumLength(5)
            .When(x => !string.IsNullOrEmpty(x.TrackingNumber))
            .WithMessage("Tracking number must be at least 5 characters long.");

        RuleFor(x => x.SenderCity)
            .NotEmpty().When(x => x.SenderCity != null)
            .WithMessage("Sender City cannot be empty if provided.");

        RuleFor(x => x.ReceiverCity)
            .NotEmpty().When(x => x.ReceiverCity != null)
            .WithMessage("Receiver City cannot be empty if provided.");

        RuleFor(x => x.ReceiverDistrict)
           .NotEmpty().When(x => x.ReceiverDistrict != null)
           .WithMessage("Receiver District cannot be empty if provided.");

        RuleFor(x => x.SenderDistrict)
           .NotEmpty().When(x => x.SenderDistrict != null)
           .WithMessage("Sender District cannot be empty if provided.");

        RuleFor(x => x.SenderStreet)
           .NotEmpty().When(x => x.SenderStreet != null)
           .WithMessage("Sender Street cannot be empty if provided.");

        RuleFor(x => x.ReceiverStreet)
           .NotEmpty().When(x => x.ReceiverStreet != null)
           .WithMessage("Receiver Street cannot be empty if provided.");

        RuleFor(x => x.SenderFullAddress)
           .NotEmpty().When(x => x.SenderFullAddress != null)
           .WithMessage("Sender Full Address cannot be empty if provided.");

        RuleFor(x => x.ReceiverFullAddress)
         .NotEmpty().When(x => x.ReceiverFullAddress != null)
         .WithMessage("Receiver Full Address cannot be empty if provided.");
    }
}