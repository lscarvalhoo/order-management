using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Commands.CreateOrder;
using OrderManagement.Application.Commands.DeleteOrder;
using OrderManagement.Application.Commands.UpdateOrderStatus;
using OrderManagement.Application.Queries.GetAllOrders;
using OrderManagement.Application.Queries.GetOrder;

namespace OrderManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllOrdersQuery();
        var orders = await _mediator.Send(query, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderQuery { OrderId = id };
        var order = await _mediator.Send(query, cancellationToken);

        if (order == null)
        {
            return NotFound(new { message = $"Order with ID {id} not found" });
        }

        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        command.OrderId = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteOrderCommand { OrderId = id };
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
