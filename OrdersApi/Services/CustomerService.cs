using Microsoft.EntityFrameworkCore;
using OrdersApi.Data;
using OrdersApi.Dtos;
using OrdersApi.Middleware;
using OrdersApi.Models;

namespace OrdersApi.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _db;

    public CustomerService(AppDbContext db) => _db = db;

    public async Task<PagedResult<CustomerDto>> GetAllAsync(CustomerQueryParameters query, CancellationToken ct = default)
    {
        IQueryable<Customer> customers = _db.Customers.AsNoTracking();

        // Пошук за ПІБ, email або телефоном
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            customers = customers.Where(c =>
                c.FullName.ToLower().Contains(term) ||
                c.Email.ToLower().Contains(term) ||
                (c.Phone != null && c.Phone.Contains(term)));
        }

        // Фільтрація
        if (query.MinOrders is not null)
            customers = customers.Where(c => c.Orders.Count >= query.MinOrders);

        // Сортування (білий список полів — довільний рядок від клієнта не потрапляє в запит)
        customers = (query.SortBy?.ToLowerInvariant()) switch
        {
            "name" => query.Desc ? customers.OrderByDescending(c => c.FullName) : customers.OrderBy(c => c.FullName),
            "email" => query.Desc ? customers.OrderByDescending(c => c.Email) : customers.OrderBy(c => c.Email),
            "createdat" => query.Desc ? customers.OrderByDescending(c => c.CreatedAt) : customers.OrderBy(c => c.CreatedAt),
            "orderscount" => query.Desc ? customers.OrderByDescending(c => c.Orders.Count) : customers.OrderBy(c => c.Orders.Count),
            _ => query.Desc ? customers.OrderByDescending(c => c.Id) : customers.OrderBy(c => c.Id)
        };

        var total = await customers.CountAsync(ct);
        var items = await customers
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(Mapping.CustomerToDto)
            .ToListAsync(ct);

        return new PagedResult<CustomerDto> { Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = total };
    }

    public Task<CustomerDto?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Customers.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(Mapping.CustomerToDto)
            .FirstOrDefaultAsync(ct);

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto, CancellationToken ct = default)
    {
        var email = Mapping.NormalizeEmail(dto.Email);
        if (await _db.Customers.AnyAsync(c => c.Email == email, ct))
            throw new ConflictException($"Клієнт з email '{email}' вже існує.");

        var customer = new Customer
        {
            FullName = dto.FullName.Trim(),
            Email = email,
            Phone = dto.Phone?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);

        return (await GetByIdAsync(customer.Id, ct))!;
    }

    public async Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto dto, CancellationToken ct = default)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (customer is null)
            return null;

        var email = Mapping.NormalizeEmail(dto.Email);
        if (await _db.Customers.AnyAsync(c => c.Email == email && c.Id != id, ct))
            throw new ConflictException($"Email '{email}' вже використовується іншим клієнтом.");

        customer.FullName = dto.FullName.Trim();
        customer.Email = email;
        customer.Phone = dto.Phone?.Trim();

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (customer is null)
            return false;

        _db.Customers.Remove(customer); // замовлення видаляються каскадно
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
