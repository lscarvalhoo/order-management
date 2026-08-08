using MediatR;
using OrderManagement.Application.DTOs;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Queries.GetAllOrders;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetAllOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var (orders, totalCount) = await _orderRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);

        var items = orders.Select(order => new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(item => new OrderItemDto
            {
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        });

        return new PagedResult<OrderDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
