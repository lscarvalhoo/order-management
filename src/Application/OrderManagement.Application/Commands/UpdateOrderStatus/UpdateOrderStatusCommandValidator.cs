using FluentValidation;

namespace OrderManagement.Application.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid order status.");
    }
}
