using Microsoft.EntityFrameworkCore;
using OrdersApi.Models;

namespace OrdersApi.Data;

/// <summary>Створює схему БД і заповнює її демонстраційними даними при першому запуску.</summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        await db.Database.EnsureCreatedAsync();

        if (await db.Customers.AnyAsync())
            return;

        logger.LogInformation("База порожня — додаю демонстраційні дані");

        var maria = new Customer { FullName = "Марія Коваленко", Email = "maria@example.com", Phone = "+380501112233" };
        var ivan = new Customer { FullName = "Іван Петренко", Email = "ivan@example.com", Phone = "+380671234567" };
        var oleh = new Customer { FullName = "Oleh Shevchenko", Email = "oleh@example.com" };

        var orders = new List<Order>
        {
            NewOrder(maria, OrderStatus.Paid, -10, ("Книга з алгоритмів", 1, 600m), ("Кава зернова", 2, 250m)),
            NewOrder(maria, OrderStatus.New, -1, ("Навушники", 1, 1500m)),
            NewOrder(ivan, OrderStatus.Shipped, -5, ("Клавіатура", 1, 2200m), ("Мишка", 1, 800m)),
            NewOrder(ivan, OrderStatus.Cancelled, -20, ("Чай зелений", 3, 120m)),
            NewOrder(oleh, OrderStatus.Completed, -30, ("Монітор 27\"", 1, 9500m))
        };

        db.Customers.AddRange(maria, ivan, oleh);
        db.Orders.AddRange(orders);
        await db.SaveChangesAsync();
    }

    private static Order NewOrder(Customer customer, OrderStatus status, int daysAgo,
        params (string Name, int Qty, decimal Price)[] items)
    {
        var order = new Order
        {
            Customer = customer,
            Status = status,
            OrderDate = DateTime.UtcNow.AddDays(daysAgo),
            OrderItems = items.Select(i => new OrderItem
            {
                ProductName = i.Name,
                Quantity = i.Qty,
                UnitPrice = i.Price
            }).ToList()
        };
        order.RecalculateTotal();
        return order;
    }
}
