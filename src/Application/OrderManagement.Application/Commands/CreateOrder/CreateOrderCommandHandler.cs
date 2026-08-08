using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;

    public CreateOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = request.Items.Select(item => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };

        var createdOrder = await _orderRepository.AddAsync(order, cancellationToken);

        return new OrderDto
        {
            Id = createdOrder.Id,
            CustomerId = createdOrder.CustomerId,
            Status = createdOrder.Status,
            CreatedAt = createdOrder.CreatedAt,
            TotalAmount = createdOrder.TotalAmount,
            Items = createdOrder.Items.Select(item => new OrderItemDto
            {
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };
    }
}
