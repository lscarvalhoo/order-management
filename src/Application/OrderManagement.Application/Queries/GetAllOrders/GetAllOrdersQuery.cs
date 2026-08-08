using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries.GetAllOrders;

public class GetAllOrdersQuery : IRequest<IEnumerable<OrderDto>>
{
}
