using System.ComponentModel.DataAnnotations;
using OrdersApi.Models;

namespace OrdersApi.Dtos;

/// <summary>Позиція замовлення у відповіді API.</summary>
public class OrderItemDto
{
    public int Id { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal => Quantity * UnitPrice;
}

/// <summary>Замовлення у відповіді API.</summary>
public class OrderDto
{
    public int Id { get; init; }
    public DateTime OrderDate { get; init; }
    public OrderStatus Status { get; init; }
    public decimal TotalAmount { get; init; }
    public int CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public List<OrderItemDto> Items { get; init; } = new();
}

/// <summary>Позиція замовлення у запиті.</summary>
public class OrderItemInputDto
{
    /// <example>Кава зернова</example>
    [Required(ErrorMessage = "Назва товару обов'язкова.")]
    [StringLength(200, MinimumLength = 1)]
    public string ProductName { get; set; } = string.Empty;

    /// <example>2</example>
    [Range(1, 1000, ErrorMessage = "Кількість має бути від 1 до 1000.")]
    public int Quantity { get; set; }

    /// <example>250</example>
    [Range(0.01, 1_000_000, ErrorMessage = "Ціна має бути від 0.01 до 1 000 000.")]
    public decimal UnitPrice { get; set; }
}

/// <summary>Дані для створення замовлення. Сума і дата рахуються сервером.</summary>
public class CreateOrderDto
{
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Вкажіть коректний CustomerId.")]
    public int CustomerId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Замовлення має містити хоча б одну позицію.")]
    [MaxLength(50, ErrorMessage = "Не більше 50 позицій.")]
    public List<OrderItemInputDto> Items { get; set; } = new();
}

/// <summary>Дані для оновлення замовлення.</summary>
public class UpdateOrderDto
{
    /// <example>Paid</example>
    [Required(ErrorMessage = "Статус обов'язковий.")]
    [EnumDataType(typeof(OrderStatus))]
    public OrderStatus? Status { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Замовлення має містити хоча б одну позицію.")]
    [MaxLength(50)]
    public List<OrderItemInputDto> Items { get; set; } = new();
}

/// <summary>Параметри фільтрації/пошуку/сортування замовлень.</summary>
public class OrderQueryParameters : QueryParametersBase, IValidatableObject
{
    /// <summary>Фільтр за клієнтом.</summary>
    public int? CustomerId { get; set; }

    /// <summary>Фільтр за статусом.</summary>
    public OrderStatus? Status { get; set; }

    /// <summary>Мінімальна сума.</summary>
    [Range(0, double.MaxValue)]
    public decimal? MinTotal { get; set; }

    /// <summary>Максимальна сума.</summary>
    [Range(0, double.MaxValue)]
    public decimal? MaxTotal { get; set; }

    /// <summary>Дата «від» (UTC).</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>Дата «до» (UTC).</summary>
    public DateTime? ToDate { get; set; }

    // SortBy: date | total | status | customer (за замовчуванням id)

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinTotal is not null && MaxTotal is not null && MinTotal > MaxTotal)
            yield return new ValidationResult("MinTotal не може бути більшим за MaxTotal.",
                new[] { nameof(MinTotal), nameof(MaxTotal) });

        if (FromDate is not null && ToDate is not null && FromDate > ToDate)
            yield return new ValidationResult("FromDate не може бути пізніше за ToDate.",
                new[] { nameof(FromDate), nameof(ToDate) });
    }
}
