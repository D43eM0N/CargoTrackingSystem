using CargoTrackingSystem.Application.Features.Shipment.Commands.CreateShipment;
using CargoTrackingSystem.Application.Validators;
using FluentValidation.TestHelper;

public class CreateShipmentValidatorTests
{
    private readonly CreateShipmentValidator _validator;

    public CreateShipmentValidatorTests()
    {
        _validator = new CreateShipmentValidator();
    }

    [Fact]
    public void Validator_TrackingNumberIsShort()
    {
        // Arrange
        var command = new CreateShipment("TR1", "Ist", "Kadikoy", "St", "34000", "Full Address", "Ist", "Uskudar", "St", "34000", "Full Address", 10.5);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(s => s.TrackingNumber);
    }

    [Fact]
    public void Validator_WeightIsZeroOrNegative()
    {   
        //In validator it coded as GreaterThanZero validation. So zero or negative will give the same result.
        // Arrange
        var command = new CreateShipment("TR12345", "Ist", "Kadikoy", "St", "34000", "Full Address", "Ist", "Uskudar", "St", "34000", "Full Address", 0);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(s => s.Weight);
    }

    [Fact]
    public void Validator_WeightExceedsLimit()
    {
        // Arrange
        var command = new CreateShipment("TR12345", "Ist", "Kadikoy", "St", "34000", "Full Address", "Ist", "Uskudar", "St", "34000", "Full Address", 155.0);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(s => s.Weight);
    }
}