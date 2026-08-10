using FluentAssertions;
using Moq;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries.GetAllOrders;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.UnitTests.Handlers;

public class GetAllOrdersQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetAllOrdersQueryHandler _handler;

    public GetAllOrdersQueryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new GetAllOrdersQueryHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithOrders_ShouldReturnPagedResult()
    {
        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Status = OrderStatus.Pending,
                CreatedAt = new DateTime(2024, 1, 1),
                Items = new List<OrderItem>
                {
                    new() { ProductName = "Product 1", Quantity = 2, UnitPrice = 10.00m }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Status = OrderStatus.Confirmed,
                CreatedAt = new DateTime(2024, 1, 2),
                Items = new List<OrderItem>
                {
                    new() { ProductName = "Product 2", Quantity = 1, UnitPrice = 25.00m }
                }
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((orders, 15));

        var query = new GetAllOrdersQuery { Page = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(2);

        var firstOrder = result.Items.First();
        firstOrder.Status.Should().Be(OrderStatus.Confirmed);
        firstOrder.TotalAmount.Should().Be(25.00m);
        firstOrder.Items.Should().HaveCount(1);

        _orderRepositoryMock.Verify(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldReturnEmptyPagedResult()
    {
        _orderRepositoryMock
            .Setup(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Order>(), 0));

        var query = new GetAllOrdersQuery { Page = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithDifferentPageSizes_ShouldReturnCorrectPagedResult()
    {
        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>()
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetPagedAsync(2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((orders, 12));

        var query = new GetAllOrdersQuery { Page = 2, PageSize = 5 };
        var result = await _handler.Handle(query, CancellationToken.None);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(12);
        result.TotalPages.Should().Be(3); // 12 / 5 = 3 pages
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationToken()
    {
        var cancellationToken = new CancellationToken();

        _orderRepositoryMock
            .Setup(x => x.GetPagedAsync(1, 10, cancellationToken))
            .ReturnsAsync((new List<Order>(), 0));

        var query = new GetAllOrdersQuery { Page = 1, PageSize = 10 };
        await _handler.Handle(query, cancellationToken);
        _orderRepositoryMock.Verify(x => x.GetPagedAsync(1, 10, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleItems_ShouldCalculateTotalAmountCorrectly()
    {
        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new() { ProductName = "Product 1", Quantity = 3, UnitPrice = 15.50m },
                    new() { ProductName = "Product 2", Quantity = 2, UnitPrice = 20.00m },
                    new() { ProductName = "Product 3", Quantity = 1, UnitPrice = 10.00m }
                }
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((orders, 1));

        var query = new GetAllOrdersQuery { Page = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);
        var order = result.Items.First();
        order.TotalAmount.Should().Be(96.50m); // (3 * 15.50) + (2 * 20.00) + (1 * 10.00)
        order.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithMixedOrderStatuses_ShouldMapAllCorrectly()
    {
        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new() { ProductName = "Product", Quantity = 1, UnitPrice = 10.00m }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Status = OrderStatus.Confirmed,
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new() { ProductName = "Product", Quantity = 1, UnitPrice = 10.00m }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Status = OrderStatus.Cancelled,
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>
                {
                    new() { ProductName = "Product", Quantity = 1, UnitPrice = 10.00m }
                }
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((orders, 3));

        var query = new GetAllOrdersQuery { Page = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);
        result.Items.Should().HaveCount(3);
        result.Items.Should().Contain(o => o.Status == OrderStatus.Pending);
        result.Items.Should().Contain(o => o.Status == OrderStatus.Confirmed);
        result.Items.Should().Contain(o => o.Status == OrderStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_WithOrdersWithoutItems_ShouldReturnZeroTotalAmount()
    {
        var orders = new List<Order>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Items = new List<OrderItem>()
            }
        };

        _orderRepositoryMock
            .Setup(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((orders, 1));

        var query = new GetAllOrdersQuery { Page = 1, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);
        var order = result.Items.First();
        order.TotalAmount.Should().Be(0);
        order.Items.Should().BeEmpty();
    }
}
