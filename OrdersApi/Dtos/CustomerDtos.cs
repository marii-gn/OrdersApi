using System.ComponentModel.DataAnnotations;

namespace OrdersApi.Dtos;

/// <summary>Клієнт у відповіді API.</summary>
public class CustomerDto
{
    public int Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public DateTime CreatedAt { get; init; }
    public int OrdersCount { get; init; }
}

/// <summary>Дані для створення клієнта.</summary>
public class CreateCustomerDto
{
    /// <example>Олена Бондар</example>
    [Required(ErrorMessage = "ПІБ обов'язкове.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "ПІБ має містити від 2 до 120 символів.")]
    public string FullName { get; set; } = string.Empty;

    /// <example>olena@example.com</example>
    [Required(ErrorMessage = "Email обов'язковий.")]
    [EmailAddress(ErrorMessage = "Некоректний формат email.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    /// <example>+380931112233</example>
    [RegularExpression(@"^\+?[0-9\- ]{7,20}$", ErrorMessage = "Некоректний номер телефону.")]
    public string? Phone { get; set; }
}

/// <summary>Дані для оновлення клієнта (повна заміна полів — семантика PUT).</summary>
public class UpdateCustomerDto : CreateCustomerDto
{
}

/// <summary>Параметри фільтрації/пошуку/сортування клієнтів.</summary>
public class CustomerQueryParameters : QueryParametersBase
{
    /// <summary>Мінімальна кількість замовлень у клієнта.</summary>
    [Range(0, int.MaxValue)]
    public int? MinOrders { get; set; }

    // SortBy: name | email | createdAt | ordersCount (за замовчуванням id)
}
