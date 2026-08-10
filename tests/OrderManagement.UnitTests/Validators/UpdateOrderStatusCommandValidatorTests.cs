using FluentAssertions;
using FluentValidation.TestHelper;
using OrderManagement.Application.Commands.UpdateOrderStatus;
using OrderManagement.Domain.Enums;

namespace OrderManagement.UnitTests.Validators;

public class UpdateOrderStatusCommandValidatorTests
{
    private readonly UpdateOrderStatusCommandValidator _validator;

    public UpdateOrderStatusCommandValidatorTests()
    {
        _validator = new UpdateOrderStatusCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        var command = new UpdateOrderStatusCommand
        {
            OrderId = Guid.NewGuid(),
            Status = OrderStatus.Confirmed
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyOrderId_ShouldHaveValidationError()
    {
        var command = new UpdateOrderStatusCommand
        {
            OrderId = Guid.Empty,
            Status = OrderStatus.Pending
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrderId)
            .WithErrorMessage("OrderId is required.");
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Cancelled)]
    public void Validate_WithValidStatuses_ShouldNotHaveValidationErrors(OrderStatus status)
    {
        var command = new UpdateOrderStatusCommand
        {
            OrderId = Guid.NewGuid(),
            Status = status
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithInvalidStatus_ShouldHaveValidationError()
    {
        var command = new UpdateOrderStatusCommand
        {
            OrderId = Guid.NewGuid(),
            Status = (OrderStatus)999 // Invalid enum value
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Invalid order status.");
    }
}
