using FluentAssertions;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.UnitTests.Domain;

public class OrderTests
{
    [Fact]
    public void TotalAmount_WithSingleItem_ShouldCalculateCorrectly()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new() { ProductName = "Product", Quantity = 3, UnitPrice = 10.50m }
            }
        };
        order.TotalAmount.Should().Be(31.50m); // 3 * 10.50
    }

    [Fact]
    public void TotalAmount_WithMultipleItems_ShouldCalculateCorrectly()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new() { ProductName = "Product 1", Quantity = 2, UnitPrice = 10.00m },
                new() { ProductName = "Product 2", Quantity = 3, UnitPrice = 15.00m },
                new() { ProductName = "Product 3", Quantity = 1, UnitPrice = 25.00m }
            }
        };
        order.TotalAmount.Should().Be(90.00m); // (2*10) + (3*15) + (1*25) = 20 + 45 + 25
    }

    [Fact]
    public void TotalAmount_WithNoItems_ShouldReturnZero()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };
        order.TotalAmount.Should().Be(0);
    }

    [Fact]
    public void TotalAmount_WithDecimalPrices_ShouldCalculateAccurately()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new() { ProductName = "Product 1", Quantity = 2, UnitPrice = 12.99m },
                new() { ProductName = "Product 2", Quantity = 3, UnitPrice = 7.50m }
            }
        };
        order.TotalAmount.Should().Be(48.48m); // (2*12.99) + (3*7.50) = 25.98 + 22.50
    }

    [Fact]
    public void TotalAmount_ShouldBeComputedProperty()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new() { ProductName = "Product", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        var initialTotal = order.TotalAmount;
        order.Items.Add(new OrderItem { ProductName = "Product 2", Quantity = 2, UnitPrice = 15.00m });
        initialTotal.Should().Be(10.00m);
        order.TotalAmount.Should().Be(40.00m); // (1*10) + (2*15)
    }

    [Fact]
    public void Order_ShouldInitializeWithEmptyItemsList()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        order.Items.Should().NotBeNull();
        order.Items.Should().BeEmpty();
        order.TotalAmount.Should().Be(0);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Cancelled)]
    public void Order_ShouldAcceptAllValidStatuses(OrderStatus status)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Status = status,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };
        order.Status.Should().Be(status);
    }

    [Fact]
    public void TotalAmount_WithLargeQuantities_ShouldCalculateCorrectly()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new() { ProductName = "Product", Quantity = 100, UnitPrice = 9.99m }
            }
        };
        order.TotalAmount.Should().Be(999.00m); // 100 * 9.99
    }
}
