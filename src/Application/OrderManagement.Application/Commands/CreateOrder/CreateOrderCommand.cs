using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Commands.CreateOrder;

public class CreateOrderCommand : IRequest<OrderDto>
{
    public Guid CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}
