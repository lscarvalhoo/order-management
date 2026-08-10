using System.Net;
using System.Net.Http.Json;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Queries.GetAllOrders;

namespace OrderManagement.IntegrationTests.Controllers;

public class OrdersControllerTests : IntegrationTestBase
{
    public OrdersControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/orders");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithAuthentication_ReturnsOk()
    {
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var response = await Client.GetAsync("/api/orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithPagination_ReturnsOkWithPaginatedResults()
    {
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var response = await Client.GetAsync("/api/orders?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<OrderDto>>();
        Assert.NotNull(result);
        Assert.True(result.PageSize <= 10);
    }

    [Fact]
    public async Task CreateOrder_WithValidData_ReturnsCreated()
    {
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var command = new
        {
            CustomerId = Guid.NewGuid(),
            Items = new[]
            {
                new
                {
                    ProductName = "Product 1",
                    Quantity = 2,
                    UnitPrice = 10.50m
                },
                new
                {
                    ProductName = "Product 2",
                    Quantity = 1,
                    UnitPrice = 25.00m
                }
            }
        };

        var response = await Client.PostAsJsonAsync("/api/orders", command);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(command.CustomerId, result.CustomerId);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(46.00m, result.TotalAmount);
    }

    [Fact]
    public async Task CreateOrder_WithoutAuthentication_ReturnsUnauthorized()
    {
        var command = new
        {
            CustomerId = Guid.NewGuid(),
            Items = new[]
            {
                new
                {
                    ProductName = "Product 1",
                    Quantity = 1,
                    UnitPrice = 10.00m
                }
            }
        };

        var response = await Client.PostAsJsonAsync("/api/orders", command);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_WithEmptyItems_ReturnsBadRequest()
    {
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var command = new
        {
            CustomerId = Guid.NewGuid(),
            Items = Array.Empty<object>()
        };

        var response = await Client.PostAsJsonAsync("/api/orders", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_WithInvalidQuantity_ReturnsBadRequest()
    {
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var command = new
        {
            CustomerId = Guid.NewGuid(),
            Items = new[]
            {
                new
                {
                    ProductName = "Product 1",
                    Quantity = 0,
                    UnitPrice = 10.00m
                }
            }
        };

        var response = await Client.PostAsJsonAsync("/api/orders", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_WithNegativePrice_ReturnsBadRequest()
    {
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var command = new
        {
            CustomerId = Guid.NewGuid(),
            Items = new[]
            {
                new
                {
                    ProductName = "Product 1",
                    Quantity = 1,
                    UnitPrice = -10.00m
                }
            }
        };

        var response = await Client.PostAsJsonAsync("/api/orders", command);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithExistingOrder_ReturnsOk()
    {
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var createCommand = new
        {
            CustomerId = Guid.NewGuid(),
            Items = new[]
            {
                new
                {
                    ProductName = "Product 1",
                    Quantity = 1,
                    UnitPrice = 10.00m
                }
            }
        };

        var createResponse = await Client.PostAsJsonAsync("/api/orders", createCommand);
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        var response = await Client.GetAsync($"/api/orders/{createdOrder!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(result);
        Assert.Equal(createdOrder.Id, result.Id);
    }

    [Fact]
    public async Task GetById_WithNonExistingOrder_ReturnsNotFound()
    {
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);
        var nonExistingId = Guid.NewGuid();

        var response = await Client.GetAsync($"/api/orders/{nonExistingId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_WithPendingOrder_ReturnsNoContent()
    {
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);

        var createCommand = new
        {
            CustomerId = Guid.NewGuid(),
            Items = new[]
            {
                new
                {
                    ProductName = "Product 1",
                    Quantity = 1,
                    UnitPrice = 10.00m
                }
            }
        };

        var createResponse = await Client.PostAsJsonAsync("/api/orders", createCommand);
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        var response = await Client.PatchAsync($"/api/orders/{createdOrder!.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getResponse = await Client.GetAsync($"/api/orders/{createdOrder.Id}");
        var cancelledOrder = await getResponse.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(cancelledOrder);
        Assert.Equal(Domain.Enums.OrderStatus.Cancelled, cancelledOrder.Status);
    }

    [Fact]
    public async Task CancelOrder_WithNonExistingOrder_ReturnsNotFound()
    {
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);
        var nonExistingId = Guid.NewGuid();

        var response = await Client.PatchAsync($"/api/orders/{nonExistingId}/cancel", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_WithoutAuthentication_ReturnsUnauthorized()
    {
        var orderId = Guid.NewGuid();

        var response = await Client.PatchAsync($"/api/orders/{orderId}/cancel", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateAndRetrieveMultipleOrders_WorksCorrectly()
    {
        var token = await GetAuthTokenAsync();
        SetAuthorizationHeader(token);
        var customerId = Guid.NewGuid();

        for (int i = 0; i < 3; i++)
        {
            var command = new
            {
                CustomerId = customerId,
                Items = new[]
                {
                    new
                    {
                        ProductName = $"Product {i}",
                        Quantity = i + 1,
                        UnitPrice = 10.00m * (i + 1)
                    }
                }
            };

            var response = await Client.PostAsJsonAsync("/api/orders", command);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        var getAllResponse = await Client.GetAsync("/api/orders?pageSize=100");

        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);
        var result = await getAllResponse.Content.ReadFromJsonAsync<PagedResult<OrderDto>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 3);
        Assert.Contains(result.Items, o => o.CustomerId == customerId);
    }
}
