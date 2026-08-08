using MediatR;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Commands.CancelOrder;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;

    public CancelOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {request.OrderId} not found.");
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException($"Only orders with status 'Pending' can be cancelled. Current status: {order.Status}");
        }

        order.Status = OrderStatus.Cancelled;
        await _orderRepository.UpdateAsync(order, cancellationToken);
    }
}