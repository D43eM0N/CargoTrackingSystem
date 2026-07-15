using CargoTrackingSystem.Application.Features.Couriers.Commands.CreateCourier;
using FluentValidation;

namespace CargoTrackingSystem.Application.Validators;

public class CreateCourierValidator : AbstractValidator<CreateCourierCommand>
{
    public CreateCourierValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First Name cannot be null or empty.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last Name cannot be null or empty.");

        RuleFor(x => x.VehicleType)
            .NotEmpty().WithMessage("Vehicle Type cannot be null or empty.");

        RuleFor(x => x.ResponsiblePostalCode)
            .NotEmpty().WithMessage("Responsible Postal Code cannot be null or empty.")
            .Length(5).WithMessage("Postal Code must be exactly 5 characters.");

        RuleFor(x => x.ActiveLoad)
            .GreaterThanOrEqualTo(0).WithMessage("Active Load cannot be negative.");
    }
}