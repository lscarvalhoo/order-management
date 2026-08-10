using FluentAssertions;
using FluentValidation.TestHelper;
using OrderManagement.Application.Commands.DeleteOrder;

namespace OrderManagement.UnitTests.Validators;

public class DeleteOrderCommandValidatorTests
{
    private readonly DeleteOrderCommandValidator _validator;

    public DeleteOrderCommandValidatorTests()
    {
        _validator = new DeleteOrderCommandValidator();
    }

    [Fact]
    public void Validate_WithValidOrderId_ShouldNotHaveValidationErrors()
    {
        var command = new DeleteOrderCommand
        {
            OrderId = Guid.NewGuid()
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyOrderId_ShouldHaveValidationError()
    {
        var command = new DeleteOrderCommand
        {
            OrderId = Guid.Empty
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrderId)
            .WithErrorMessage("OrderId is required.");
    }
}
