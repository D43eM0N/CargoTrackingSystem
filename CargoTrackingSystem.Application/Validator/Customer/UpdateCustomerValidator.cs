using CargoTrackingSystem.Application.Features.Customers.Commands.UpdateCustomer;
using FluentValidation;

namespace CargoTrackingSystem.Application.Validators;

public class UpdateCustomerValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().When(x => x.FullName != null)
            .WithMessage("Full Name cannot be empty if provided.");

        RuleFor(x => x.Email)
            .NotEmpty().When(x => x.Email != null)
            .EmailAddress().When(x => x.Email != null)
            .WithMessage("Email must be a valid email format if provided.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().When(x => x.PhoneNumber != null)
            .WithMessage("Phone Number cannot be empty if provided.");
    }
}