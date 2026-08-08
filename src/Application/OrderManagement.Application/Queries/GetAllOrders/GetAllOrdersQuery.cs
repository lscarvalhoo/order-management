using MediatR;
using OrderManagement.Application.DTOs;

namespace OrderManagement.Application.Queries.GetAllOrders;

public class GetAllOrdersQuery : IRequest<PagedResult<OrderDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
