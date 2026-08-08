using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount => Items.Sum(i => i.UnitPrice * i.Quantity);
}
