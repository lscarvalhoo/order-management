using FluentAssertions;
using Moq;
using OrderManagement.Application.Commands.CreateOrder;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.UnitTests.Handlers;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _mockRepository;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _mockRepository = new Mock<IOrderRepository>();
        _handler = new CreateOrderCommandHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateOrderWithPendingStatus()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new() { ProductName = "Product 1", Quantity = 2, UnitPrice = 10.50m },
                new() { ProductName = "Product 2", Quantity = 1, UnitPrice = 25.00m }
            }
        };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order order, CancellationToken ct) => order);
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(command.CustomerId);
        result.Status.Should().Be(OrderStatus.Pending);
        result.Items.Should().HaveCount(2);
        result.TotalAmount.Should().Be(46.00m);

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldSetCreatedAtToUtcNow()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new() { ProductName = "Product 1", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order order, CancellationToken ct) => order);

        var beforeExecution = DateTime.UtcNow;
        var result = await _handler.Handle(command, CancellationToken.None);

        var afterExecution = DateTime.UtcNow;
        result.CreatedAt.Should().BeOnOrAfter(beforeExecution).And.BeOnOrBefore(afterExecution);
    }

    [Fact]
    public async Task Handle_WithSingleItem_ShouldCalculateTotalAmountCorrectly()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new() { ProductName = "Product", Quantity = 3, UnitPrice = 15.50m }
            }
        };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order order, CancellationToken ct) => order);
        var result = await _handler.Handle(command, CancellationToken.None);
        result.TotalAmount.Should().Be(46.50m); // 3 * 15.50
    }

    [Fact]
    public async Task Handle_WithMultipleItems_ShouldMapAllItemsCorrectly()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new() { ProductName = "Product A", Quantity = 1, UnitPrice = 10.00m },
                new() { ProductName = "Product B", Quantity = 2, UnitPrice = 20.00m },
                new() { ProductName = "Product C", Quantity = 3, UnitPrice = 30.00m }
            }
        };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order order, CancellationToken ct) => order);
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Items.Should().HaveCount(3);
        result.Items[0].ProductName.Should().Be("Product A");
        result.Items[0].Quantity.Should().Be(1);
        result.Items[0].UnitPrice.Should().Be(10.00m);
        result.Items[1].ProductName.Should().Be("Product B");
        result.Items[1].Quantity.Should().Be(2);
        result.Items[1].UnitPrice.Should().Be(20.00m);
        result.Items[2].ProductName.Should().Be("Product C");
        result.Items[2].Quantity.Should().Be(3);
        result.Items[2].UnitPrice.Should().Be(30.00m);
        result.TotalAmount.Should().Be(140.00m); // 10 + 40 + 90
    }

    [Fact]
    public async Task Handle_ShouldGenerateNewOrderId()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new() { ProductName = "Product", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order order, CancellationToken ct) => order);
        var result = await _handler.Handle(command, CancellationToken.None);
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new() { ProductName = "Product", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        var cancellationToken = new CancellationToken();

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>(), cancellationToken))
            .ReturnsAsync((Order order, CancellationToken ct) => order);
        await _handler.Handle(command, cancellationToken);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Order>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateOrderWithCorrectCustomerId()
    {
        var expectedCustomerId = Guid.NewGuid();
        var command = new CreateOrderCommand
        {
            CustomerId = expectedCustomerId,
            Items = new List<OrderItemDto>
            {
                new() { ProductName = "Product", Quantity = 1, UnitPrice = 10.00m }
            }
        };

        Order capturedOrder = null!;
        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, ct) => capturedOrder = order)
            .ReturnsAsync((Order order, CancellationToken ct) => order);
        await _handler.Handle(command, CancellationToken.None);
        capturedOrder.Should().NotBeNull();
        capturedOrder.CustomerId.Should().Be(expectedCustomerId);
    }

    [Fact]
    public async Task Handle_WithDecimalPrices_ShouldCalculateTotalAmountAccurately()
    {
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemDto>
            {
                new() { ProductName = "Product 1", Quantity = 2, UnitPrice = 12.99m },
                new() { ProductName = "Product 2", Quantity = 3, UnitPrice = 7.50m }
            }
        };

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order order, CancellationToken ct) => order);
        var result = await _handler.Handle(command, CancellationToken.None);
        result.TotalAmount.Should().Be(48.48m); // (2 * 12.99) + (3 * 7.50)
    }
}
