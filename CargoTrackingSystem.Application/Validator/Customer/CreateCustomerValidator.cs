using CargoTrackingSystem.Application.Features.Customers.Commands.CreateCustomer;
using FluentValidation;

namespace CargoTrackingSystem.Application.Validators;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full Name cannot be null or empty.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email cannot be null or empty.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone Number cannot be null or empty.");
    }
}