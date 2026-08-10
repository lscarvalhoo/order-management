using FluentValidation.TestHelper;
using OrderManagement.Application.Commands.CancelOrder;

namespace OrderManagement.UnitTests.Validators;

public class CancelOrderCommandValidatorTests
{
    private readonly CancelOrderCommandValidator _validator;

    public CancelOrderCommandValidatorTests()
    {
        _validator = new CancelOrderCommandValidator();
    }

    [Fact]
    public void Validate_WithValidOrderId_ShouldNotHaveValidationErrors()
    {
        var command = new CancelOrderCommand
        {
            OrderId = Guid.NewGuid()
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyOrderId_ShouldHaveValidationError()
    {
        var command = new CancelOrderCommand
        {
            OrderId = Guid.Empty
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrderId)
            .WithErrorMessage("OrderId is required.");
    }
}
