using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries.GetOrder;

public class GetOrderQuery : IRequest<OrderDto?>
{
    public Guid OrderId { get; set; }
}
