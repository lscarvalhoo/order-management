using FluentAssertions;
using MediatR;
using Moq;
using OrderManagement.Application.Commands.UpdateOrderStatus;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.UnitTests.Handlers;

public class UpdateOrderStatusCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly UpdateOrderStatusCommandHandler _handler;

    public UpdateOrderStatusCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new UpdateOrderStatusCommandHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingOrder_ShouldUpdateStatus()
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
                new() { ProductName = "Product", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = orderId,
            Status = OrderStatus.Confirmed
        };
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Should().Be(Unit.Value);
        order.Status.Should().Be(OrderStatus.Confirmed);
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

        var command = new UpdateOrderStatusCommand
        {
            OrderId = orderId,
            Status = OrderStatus.Confirmed
        };
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Order with ID {orderId} not found.");

        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Pending)]
    public async Task Handle_WithDifferentStatusTransitions_ShouldUpdateStatus(OrderStatus fromStatus, OrderStatus toStatus)
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = fromStatus,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>
            {
                new() { ProductName = "Product", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = orderId,
            Status = toStatus
        };
        await _handler.Handle(command, CancellationToken.None);
        order.Status.Should().Be(toStatus);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
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

        _orderRepositoryMock
            .Setup(x => x.UpdateAsync(order, cancellationToken))
            .Returns(Task.CompletedTask);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = orderId,
            Status = OrderStatus.Confirmed
        };
        await _handler.Handle(command, cancellationToken);
        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId, cancellationToken), Times.Once);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(order, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSameStatus_ShouldStillUpdateOrder()
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

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = orderId,
            Status = OrderStatus.Pending // Same as current status
        };
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Should().Be(Unit.Value);
        order.Status.Should().Be(OrderStatus.Pending);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithOrderWithMultipleItems_ShouldUpdateStatus()
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
                new() { ProductName = "Product 1", Quantity = 2, UnitPrice = 10.00m },
                new() { ProductName = "Product 2", Quantity = 3, UnitPrice = 15.00m },
                new() { ProductName = "Product 3", Quantity = 1, UnitPrice = 20.00m }
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = orderId,
            Status = OrderStatus.Confirmed
        };
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Should().Be(Unit.Value);
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.Items.Should().HaveCount(3);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }
}
