using OrdersApi.Dtos;

namespace OrdersApi.Services;

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetAllAsync(CustomerQueryParameters query, CancellationToken ct = default);
    Task<CustomerDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto, CancellationToken ct = default);
    Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
