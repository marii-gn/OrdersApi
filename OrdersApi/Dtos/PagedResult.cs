using System.ComponentModel.DataAnnotations;

namespace OrdersApi.Dtos;

/// <summary>Сторінка результатів списку.</summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>Спільні параметри пагінації та сортування.</summary>
public abstract class QueryParametersBase
{
    /// <summary>Номер сторінки (від 1).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Номер сторінки має бути >= 1.")]
    public int Page { get; set; } = 1;

    /// <summary>Розмір сторінки (1–100).</summary>
    [Range(1, 100, ErrorMessage = "Розмір сторінки має бути від 1 до 100.")]
    public int PageSize { get; set; } = 10;

    /// <summary>Пошуковий рядок.</summary>
    [StringLength(100)]
    public string? Search { get; set; }

    /// <summary>Поле сортування.</summary>
    public string? SortBy { get; set; }

    /// <summary>true — сортування за спаданням.</summary>
    public bool Desc { get; set; }
}
