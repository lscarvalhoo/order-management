using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.DTOs;

public sealed class OrderDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
}
