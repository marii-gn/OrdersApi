using Microsoft.EntityFrameworkCore;
using OrdersApi.Data;
using OrdersApi.Dtos;
using OrdersApi.Middleware;
using OrdersApi.Models;

namespace OrdersApi.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db) => _db = db;

    public async Task<PagedResult<OrderDto>> GetAllAsync(OrderQueryParameters query, CancellationToken ct = default)
    {
        IQueryable<Order> orders = _db.Orders.AsNoTracking();

        // --- Фільтрація ---
        if (query.CustomerId is not null)
            orders = orders.Where(o => o.CustomerId == query.CustomerId);
        if (query.Status is not null)
            orders = orders.Where(o => o.Status == query.Status);
        if (query.MinTotal is not null)
            orders = orders.Where(o => o.TotalAmount >= query.MinTotal.Value);
        if (query.MaxTotal is not null)
            orders = orders.Where(o => o.TotalAmount <= query.MaxTotal.Value);
        if (query.FromDate is not null)
        {
            var from = AsUtc(query.FromDate.Value);
            orders = orders.Where(o => o.OrderDate >= from);
        }
        if (query.ToDate is not null)
        {
            var to = AsUtc(query.ToDate.Value);
            orders = orders.Where(o => o.OrderDate <= to);
        }

        // --- Пошук: за ім'ям клієнта або назвою товару ---
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            orders = orders.Where(o =>
                o.Customer.FullName.ToLower().Contains(term) ||
                o.OrderItems.Any(i => i.ProductName.ToLower().Contains(term)));
        }

        // --- Сортування ---
        orders = (query.SortBy?.ToLowerInvariant()) switch
        {
            "date" => query.Desc ? orders.OrderByDescending(o => o.OrderDate) : orders.OrderBy(o => o.OrderDate),
            "total" => query.Desc ? orders.OrderByDescending(o => o.TotalAmount) : orders.OrderBy(o => o.TotalAmount),
            "status" => query.Desc ? orders.OrderByDescending(o => o.Status) : orders.OrderBy(o => o.Status),
            "customer" => query.Desc ? orders.OrderByDescending(o => o.Customer.FullName) : orders.OrderBy(o => o.Customer.FullName),
            _ => query.Desc ? orders.OrderByDescending(o => o.Id) : orders.OrderBy(o => o.Id)
        };

        var total = await orders.CountAsync(ct);
        var items = await orders
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(Mapping.OrderToDto)
            .ToListAsync(ct);

        return new PagedResult<OrderDto> { Items = items, Page = query.Page, PageSize = query.PageSize, TotalCount = total };
    }

    public Task<OrderDto?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Orders.AsNoTracking()
            .Where(o => o.Id == id)
            .Select(Mapping.OrderToDto)
            .FirstOrDefaultAsync(ct);

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto, CancellationToken ct = default)
    {
        if (!await _db.Customers.AnyAsync(c => c.Id == dto.CustomerId, ct))
            throw new BadRequestException($"Клієнта з id={dto.CustomerId} не існує.");

        var order = new Order
        {
            CustomerId = dto.CustomerId,
            OrderDate = DateTime.UtcNow,     // дату ставить сервер
            Status = OrderStatus.New,         // нове замовлення завжди має статус New
            OrderItems = dto.Items.ToEntities()
        };
        order.RecalculateTotal();             // суму рахує сервер

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        return (await GetByIdAsync(order.Id, ct))!;
    }

    public async Task<OrderDto?> UpdateAsync(int id, UpdateOrderDto dto, CancellationToken ct = default)
    {
        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
            return null;

        // Бізнес-правило: завершені та скасовані замовлення не редагуються
        if (order.Status is OrderStatus.Completed or OrderStatus.Cancelled)
            throw new ConflictException($"Замовлення у статусі {order.Status} не можна змінювати.");

        order.Status = dto.Status!.Value;

        // Повна заміна позицій (семантика PUT). Старі позиції видаляються як «сироти».
        order.OrderItems.Clear();
        order.OrderItems.AddRange(dto.Items.ToEntities());
        order.RecalculateTotal();

        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
            return false;

        _db.Orders.Remove(order);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // PostgreSQL (timestamptz) вимагає DateTime з Kind=Utc
    private static DateTime AsUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
