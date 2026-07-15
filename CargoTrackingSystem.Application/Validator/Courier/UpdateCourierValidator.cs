using CargoTrackingSystem.Application.Features.Couriers.Commands.UpdateCourier;
using FluentValidation;

namespace CargoTrackingSystem.Application.Validators;

public class UpdateCourierValidator : AbstractValidator<UpdateCourierCommand>
{
    public UpdateCourierValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().When(x => x.FirstName != null)
            .WithMessage("First Name cannot be empty if provided.");

        RuleFor(x => x.LastName)
            .NotEmpty().When(x => x.LastName != null)
            .WithMessage("Last Name cannot be empty if provided.");

        RuleFor(x => x.VehicleType)
            .NotEmpty().When(x => x.VehicleType != null)
            .WithMessage("Vehicle Type cannot be empty if provided.");

        RuleFor(x => x.ResponsiblePostalCode)
            .NotEmpty().When(x => x.ResponsiblePostalCode != null)
            .Length(5).When(x => x.ResponsiblePostalCode != null)
            .WithMessage("Responsible Postal Code must be exactly 5 characters if provided.");
    }
}