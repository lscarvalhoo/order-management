using MediatR;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommand : IRequest<Unit>
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
}
