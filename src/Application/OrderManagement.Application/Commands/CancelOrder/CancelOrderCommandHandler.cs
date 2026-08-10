using MediatR;
using OrderManagement.Application.Telemetry;
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
        using var activity = ApplicationActivitySource.StartActivity("CancelOrder");
        activity?.SetTag("order.id", request.OrderId);

        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order == null)
        {
            activity?.SetTag("order.found", false);
            throw new KeyNotFoundException($"Order with ID {request.OrderId} not found.");
        }

        activity?.SetTag("order.found", true);
        activity?.SetTag("order.current_status", order.Status.ToString());

        if (order.Status != OrderStatus.Pending)
        {
            activity?.SetTag("order.cancellation_allowed", false);
            throw new InvalidOperationException($"Only orders with status 'Pending' can be cancelled. Current status: {order.Status}");
        }

        activity?.SetTag("order.cancellation_allowed", true);
        order.Status = OrderStatus.Cancelled;
        await _orderRepository.UpdateAsync(order, cancellationToken);

        activity?.SetTag("order.new_status", order.Status.ToString());
    }
}
