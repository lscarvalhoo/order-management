using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Queries.GetOrder;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order == null)
        {
            return null;
        }

        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(item => new OrderItemDto
            {
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };
    }
}
