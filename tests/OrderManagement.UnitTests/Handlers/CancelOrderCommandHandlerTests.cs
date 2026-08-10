using FluentAssertions;
using Moq;
using OrderManagement.Application.Commands.CancelOrder;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.UnitTests.Handlers;

public class CancelOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new CancelOrderCommandHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidPendingOrder_ShouldCancelOrder()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new() { ProductName = "Product 1", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CancelOrderCommand { OrderId = orderId };
        await _handler.Handle(command, CancellationToken.None);
        order.Status.Should().Be(OrderStatus.Cancelled);
        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingOrder_ShouldThrowKeyNotFoundException()
    {
        var orderId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new CancelOrderCommand { OrderId = orderId };
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Order with ID {orderId} not found.");

        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task Handle_WithNonPendingOrder_ShouldThrowInvalidOperationException(OrderStatus status)
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = status,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new() { ProductName = "Product 1", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var command = new CancelOrderCommand { OrderId = orderId };
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Only orders with status 'Pending' can be cancelled. Current status: {status}");

        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
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
            Items = new List<OrderItem>
            {
                new() { ProductName = "Product 1", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        var cancellationToken = new CancellationToken();

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, cancellationToken))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(x => x.UpdateAsync(order, cancellationToken))
            .Returns(Task.CompletedTask);

        var command = new CancelOrderCommand { OrderId = orderId };
        await _handler.Handle(command, cancellationToken);
        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId, cancellationToken), Times.Once);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(order, cancellationToken), Times.Once);
    }
}
