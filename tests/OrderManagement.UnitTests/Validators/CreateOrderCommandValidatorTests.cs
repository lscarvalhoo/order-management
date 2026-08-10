using FluentValidation.TestHelper;
using OrderManagement.Application.Commands.CreateOrder;
using OrderManagement.Application.DTOs;

namespace OrderManagement.UnitTests.Validators;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator;

    public CreateOrderCommandValidatorTests()
    {
        _validator = new CreateOrderCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new() { ProductName = "Product 1", Quantity = 1, UnitPrice = 10.00m }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyCustomerId_ShouldHaveValidationError()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.Empty,
            Items = new List<OrderItemDto>
            {
                new() { ProductName = "Product 1", Quantity = 1, UnitPrice = 10.00m }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorMessage("CustomerId is required.");
    }

    [Fact]
    public void Validate_WithEmptyItems_ShouldHaveValidationError()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>()
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage("Order must have at least one item.");
    }

    [Fact]
    public void Validate_WithEmptyProductName_ShouldHaveValidationError()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new() { ProductName = "", Quantity = 1, UnitPrice = 10.00m }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Items[0].ProductName")
            .WithErrorMessage("Product name is required.");
    }

    [Fact]
    public void Validate_WithProductNameTooLong_ShouldHaveValidationError()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new() { ProductName = new string('A', 201), Quantity = 1, UnitPrice = 10.00m }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Items[0].ProductName")
            .WithErrorMessage("Product name must not exceed 200 characters.");
    }

    [Fact]
    public void Validate_WithZeroQuantity_ShouldHaveValidationError()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new() { ProductName = "Product", Quantity = 0, UnitPrice = 10.00m }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Items[0].Quantity")
            .WithErrorMessage("Quantity must be greater than 0.");
    }

    [Fact]
    public void Validate_WithNegativeQuantity_ShouldHaveValidationError()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new() { ProductName = "Product", Quantity = -1, UnitPrice = 10.00m }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Items[0].Quantity")
            .WithErrorMessage("Quantity must be greater than 0.");
    }

    [Fact]
    public void Validate_WithZeroUnitPrice_ShouldHaveValidationError()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new() { ProductName = "Product", Quantity = 1, UnitPrice = 0m }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Items[0].UnitPrice")
            .WithErrorMessage("Unit price must be greater than 0.");
    }

    [Fact]
    public void Validate_WithNegativeUnitPrice_ShouldHaveValidationError()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new() { ProductName = "Product", Quantity = 1, UnitPrice = -10.00m }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Items[0].UnitPrice")
            .WithErrorMessage("Unit price must be greater than 0.");
    }

    [Fact]
    public void Validate_WithMultipleItemsAndMixedValidation_ShouldHaveCorrectErrors()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new() { ProductName = "Valid Product", Quantity = 1, UnitPrice = 10.00m },
                new() { ProductName = "", Quantity = 0, UnitPrice = -5.00m }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Items[1].ProductName");
        result.ShouldHaveValidationErrorFor("Items[1].Quantity");
        result.ShouldHaveValidationErrorFor("Items[1].UnitPrice");
        result.ShouldNotHaveValidationErrorFor("Items[0].ProductName");
        result.ShouldNotHaveValidationErrorFor("Items[0].Quantity");
        result.ShouldNotHaveValidationErrorFor("Items[0].UnitPrice");
    }

    [Fact]
    public void Validate_WithValidProductNameAtMaxLength_ShouldNotHaveValidationError()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new() { ProductName = new string('A', 200), Quantity = 1, UnitPrice = 10.00m }
            }
        };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor("Items[0].ProductName");
    }
}
