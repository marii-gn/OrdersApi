namespace OrdersApi.Models;

/// <summary>Статус замовлення.</summary>
public enum OrderStatus
{
    New,
    Paid,
    Shipped,
    Completed,
    Cancelled
}
