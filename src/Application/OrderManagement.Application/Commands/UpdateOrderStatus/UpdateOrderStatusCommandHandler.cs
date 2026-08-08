using MediatR;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Unit>
{
    private readonly IOrderRepository _orderRepository;

    public UpdateOrderStatusCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Unit> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {request.OrderId} not found.");
        }

        order.Status = request.Status;
        await _orderRepository.UpdateAsync(order, cancellationToken);

        return Unit.Value;
    }
}
