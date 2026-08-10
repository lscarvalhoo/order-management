using FluentAssertions;
using Moq;
using OrderManagement.Application.Queries.GetOrder;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.UnitTests.Handlers;

public class GetOrderQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetOrderQueryHandler _handler;

    public GetOrderQueryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new GetOrderQueryHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingOrder_ShouldReturnOrderDto()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            CreatedAt = new DateTime(2024, 1, 1),
            Items = new List<OrderItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Product 1",
                    Quantity = 2,
                    UnitPrice = 10.50m
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductName = "Product 2",
                    Quantity = 1,
                    UnitPrice = 25.00m
                }
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var query = new GetOrderQuery { OrderId = orderId };
        var result = await _handler.Handle(query, CancellationToken.None);
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.CustomerId.Should().Be(customerId);
        result.Status.Should().Be(OrderStatus.Pending);
        result.CreatedAt.Should().Be(new DateTime(2024, 1, 1));
        result.TotalAmount.Should().Be(46.00m); // (2 * 10.50) + (1 * 25.00)
        result.Items.Should().HaveCount(2);
        result.Items[0].ProductName.Should().Be("Product 1");
        result.Items[0].Quantity.Should().Be(2);
        result.Items[0].UnitPrice.Should().Be(10.50m);
        result.Items[1].ProductName.Should().Be("Product 2");
        result.Items[1].Quantity.Should().Be(1);
        result.Items[1].UnitPrice.Should().Be(25.00m);

        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingOrder_ShouldReturnNull()
    {
        var orderId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var query = new GetOrderQuery { OrderId = orderId };
        var result = await _handler.Handle(query, CancellationToken.None);
        result.Should().BeNull();
        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithOrderWithoutItems_ShouldReturnOrderDtoWithEmptyItems()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Confirmed,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var query = new GetOrderQuery { OrderId = orderId };
        var result = await _handler.Handle(query, CancellationToken.None);
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalAmount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };

        var cancellationToken = new CancellationToken();

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, cancellationToken))
            .ReturnsAsync(order);

        var query = new GetOrderQuery { OrderId = orderId };
        await _handler.Handle(query, cancellationToken);
        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDifferentOrderStatuses_ShouldMapCorrectly()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Cancelled,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new() { ProductName = "Product", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var query = new GetOrderQuery { OrderId = orderId };
        var result = await _handler.Handle(query, CancellationToken.None);
        result.Should().NotBeNull();
        result!.Status.Should().Be(OrderStatus.Cancelled);
    }
}
