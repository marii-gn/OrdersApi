using System.Linq.Expressions;
using OrdersApi.Dtos;
using OrdersApi.Models;

namespace OrdersApi.Services;

/// <summary>
/// Проєкції сутність → DTO. Вирази (Expression) транслюються EF Core у SQL,
/// тому з БД вибираються лише потрібні колонки.
/// </summary>
public static class Mapping
{
    public static readonly Expression<Func<Customer, CustomerDto>> CustomerToDto = c => new CustomerDto
    {
        Id = c.Id,
        FullName = c.FullName,
        Email = c.Email,
        Phone = c.Phone,
        CreatedAt = c.CreatedAt,
        OrdersCount = c.Orders.Count
    };

    public static readonly Expression<Func<Order, OrderDto>> OrderToDto = o => new OrderDto
    {
        Id = o.Id,
        OrderDate = o.OrderDate,
        Status = o.Status,
        TotalAmount = o.TotalAmount,
        CustomerId = o.CustomerId,
        CustomerName = o.Customer.FullName,
        Items = o.OrderItems
            .OrderBy(i => i.Id)
            .Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            })
            .ToList()
    };

    public static List<OrderItem> ToEntities(this IEnumerable<OrderItemInputDto> items) =>
        items.Select(i => new OrderItem
        {
            ProductName = i.ProductName.Trim(),
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice
        }).ToList();

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
