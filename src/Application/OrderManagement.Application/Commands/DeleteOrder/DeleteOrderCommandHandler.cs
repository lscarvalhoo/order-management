using MediatR;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Commands.DeleteOrder;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Unit>
{
    private readonly IOrderRepository _orderRepository;

    public DeleteOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Unit> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {request.OrderId} not found.");
        }

        await _orderRepository.DeleteAsync(request.OrderId, cancellationToken);

        return Unit.Value;
    }
}
