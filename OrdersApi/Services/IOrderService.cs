using OrdersApi.Dtos;

namespace OrdersApi.Services;

public interface IOrderService
{
    Task<PagedResult<OrderDto>> GetAllAsync(OrderQueryParameters query, CancellationToken ct = default);
    Task<OrderDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<OrderDto> CreateAsync(CreateOrderDto dto, CancellationToken ct = default);
    Task<OrderDto?> UpdateAsync(int id, UpdateOrderDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
