using CargoTrackingSystem.Application.Features.Shipments.Commands.UpdateShipment;
using CargoTrackingSystem.Application.Validators;
using FluentValidation.TestHelper;

public class UpdateShipmentValidatorTests
{
    private readonly UpdateShipmentValidator _validator;

    public UpdateShipmentValidatorTests()
    {
        _validator = new UpdateShipmentValidator();
    }

    [Fact]
    public void WhenWeightIsNegativeOrZero_ShouldHaveValidationError()
    {
        // Arrange
        //Should give an error about Weight when it's negative or zero
        var command = new UpdateShipment(
            Id: Guid.NewGuid(),
            TrackingNumber: "TR12345",
            Weight: 0, // Invalid weight
            SenderCity: "Istanbul", SenderDistrict: "Kadikoy", SenderStreet: "St", SenderPostalCode: "34000", SenderFullAddress: "Full Address",
            ReceiverCity: "Ankara", ReceiverDistrict: "Cankaya", ReceiverStreet: "St", ReceiverPostalCode: "06000", ReceiverFullAddress: "Full Address"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Weight)
              .WithErrorMessage("Cargo Weight must be more than 0.");
    }

    [Fact]
    public void WhenWeightExceedsMaximum_ShouldHaveValidationError()
    {
        // Arrange
        //Should give an error about Weight when more than 150KG
        var command = new UpdateShipment(
            Id: Guid.NewGuid(),
            TrackingNumber: "TR12345",
            Weight: 155.5, //Exceeding the weight limit
            SenderCity: "Istanbul", SenderDistrict: "Kadikoy", SenderStreet: "St", SenderPostalCode: "34000", SenderFullAddress: "Full Address",
            ReceiverCity: "Ankara", ReceiverDistrict: "Cankaya", ReceiverStreet: "St", ReceiverPostalCode: "06000", ReceiverFullAddress: "Full Address"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Weight)
              .WithErrorMessage("Maximum 150KG is Accepted at Once.");
    }

    [Fact]
    public void WhenTrackingNumberIsTooShort_ShouldHaveValidationError()
    {
        // Arrange: 
        //If a tracking number is provided but is shorter than 5 characters, it should throw an error
        var command = new UpdateShipment(
            Id: Guid.NewGuid(),
            TrackingNumber: "TR1", // 3-digit
            Weight: 10.0,
            SenderCity: "Istanbul", SenderDistrict: "Kadikoy", SenderStreet: "St", SenderPostalCode: "34000", SenderFullAddress: "Full Address",
            ReceiverCity: "Ankara", ReceiverDistrict: "Cankaya", ReceiverStreet: "St", ReceiverPostalCode: "06000", ReceiverFullAddress: "Full Address"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TrackingNumber)
              .WithErrorMessage("Tracking number must be at least 5 characters long.");
    }

    [Fact]
    public void WhenSenderCityIsEmptyString_ShouldHaveValidationError()
    {
        // Arrange: 
        //It should throw an error if the city field is not null but is sent as empty ("")
        var command = new UpdateShipment(
            Id: Guid.NewGuid(),
            TrackingNumber: "TR12345",
            Weight: 10.0,
            SenderCity: "", // Invalid empty string
            SenderDistrict: "Kadikoy", SenderStreet: "St", SenderPostalCode: "34000", SenderFullAddress: "Full Address",
            ReceiverCity: "Ankara", ReceiverDistrict: "Cankaya", ReceiverStreet: "St", ReceiverPostalCode: "06000", ReceiverFullAddress: "Full Address"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SenderCity)
              .WithErrorMessage("Sender City cannot be empty if provided.");
    }

    [Fact]
    public void Validator_WhenOptionalFieldsAreNull_ShouldBeValid()
    {
        // Arrange
        //Fields left null should not trigger the rule
        //they should be considered valid 
        var command = new UpdateShipment(
            Id: Guid.NewGuid(),
            TrackingNumber: null, 
            Weight: null,        
            SenderCity: null, SenderDistrict: null, SenderStreet: null, SenderPostalCode: null, SenderFullAddress: null,
            ReceiverCity: "Ankara", ReceiverDistrict: "Cankaya", ReceiverStreet: "St", ReceiverPostalCode: "06000", ReceiverFullAddress: "Full Address" 
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
