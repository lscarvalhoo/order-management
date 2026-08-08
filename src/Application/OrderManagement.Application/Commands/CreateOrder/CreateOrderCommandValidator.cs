using FluentValidation;

namespace OrderManagement.Application.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must have at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            item.RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("Unit price must be greater than 0.");
        });
    }
}
