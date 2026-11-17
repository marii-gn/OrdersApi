namespace OrdersApi.Middleware;

/// <summary>Базовий клас прикладних помилок, які безпечно показувати клієнту.</summary>
public abstract class AppException : Exception
{
    protected AppException(int statusCode, string title, string message) : base(message)
    {
        StatusCode = statusCode;
        Title = title;
    }

    public int StatusCode { get; }
    public string Title { get; }
}

/// <summary>400 — запит суперечить бізнес-правилам (напр. неіснуючий клієнт).</summary>
public sealed class BadRequestException(string message)
    : AppException(StatusCodes.Status400BadRequest, "Некоректний запит", message);

/// <summary>404 — ресурс не знайдено.</summary>
public sealed class NotFoundException(string message)
    : AppException(StatusCodes.Status404NotFound, "Ресурс не знайдено", message);

/// <summary>409 — конфлікт стану (дублікат, недопустима зміна статусу).</summary>
public sealed class ConflictException(string message)
    : AppException(StatusCodes.Status409Conflict, "Конфлікт", message);
