using MediatR;

namespace OrderManagement.Application.Commands.CancelOrder;

public class CancelOrderCommand : IRequest
{
    public Guid OrderId { get; set; }
}
