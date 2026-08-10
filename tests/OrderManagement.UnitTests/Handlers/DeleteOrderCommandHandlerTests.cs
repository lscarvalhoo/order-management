using FluentAssertions;
using MediatR;
using Moq;
using OrderManagement.Application.Commands.DeleteOrder;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.UnitTests.Handlers;

public class DeleteOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly DeleteOrderCommandHandler _handler;

    public DeleteOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new DeleteOrderCommandHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingOrder_ShouldDeleteOrder()
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
            .Setup(x => x.DeleteAsync(orderId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteOrderCommand { OrderId = orderId };
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Should().Be(Unit.Value);
        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.DeleteAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingOrder_ShouldThrowKeyNotFoundException()
    {
        var orderId = Guid.NewGuid();

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var command = new DeleteOrderCommand { OrderId = orderId };
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Order with ID {orderId} not found.");

        _orderRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
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
            .Setup(x => x.DeleteAsync(orderId, cancellationToken))
            .Returns(Task.CompletedTask);

        var command = new DeleteOrderCommand { OrderId = orderId };
        await _handler.Handle(command, cancellationToken);
        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId, cancellationToken), Times.Once);
        _orderRepositoryMock.Verify(x => x.DeleteAsync(orderId, cancellationToken), Times.Once);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task Handle_WithAnyOrderStatus_ShouldDeleteOrder(OrderStatus status)
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = status,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(x => x.DeleteAsync(orderId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteOrderCommand { OrderId = orderId };
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Should().Be(Unit.Value);
        _orderRepositoryMock.Verify(x => x.DeleteAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithOrderWithItems_ShouldDeleteOrder()
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
                new() { ProductName = "Product 2", Quantity = 3, UnitPrice = 15.00m }
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(x => x.DeleteAsync(orderId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteOrderCommand { OrderId = orderId };
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Should().Be(Unit.Value);
        _orderRepositoryMock.Verify(x => x.DeleteAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
