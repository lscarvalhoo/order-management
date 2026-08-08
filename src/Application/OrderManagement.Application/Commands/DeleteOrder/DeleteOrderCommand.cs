using MediatR;

namespace OrderManagement.Application.Commands.DeleteOrder;

public class DeleteOrderCommand : IRequest<Unit>
{
    public Guid OrderId { get; set; }
}
