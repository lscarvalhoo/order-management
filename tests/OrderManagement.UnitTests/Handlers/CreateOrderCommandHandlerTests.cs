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
        // Arrange
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

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
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
        // Arrange
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

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        var afterExecution = DateTime.UtcNow;

        // Assert
        result.CreatedAt.Should().BeOnOrAfter(beforeExecution).And.BeOnOrBefore(afterExecution);
    }
}
