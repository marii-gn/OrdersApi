namespace OrdersApi.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public decimal TotalAmount { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public List<OrderItem> OrderItems { get; set; } = new();

    /// <summary>Сума замовлення завжди рахується на сервері, а не приймається від клієнта.</summary>
    public void RecalculateTotal() =>
        TotalAmount = OrderItems.Sum(i => i.Quantity * i.UnitPrice);
}
