using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OrdersApi.Middleware;

/// <summary>
/// Єдина точка обробки винятків. Перетворює їх на ProblemDetails (RFC 7807)
/// і не віддає клієнту stack trace та інші внутрішні деталі.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetails;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetails)
    {
        _logger = logger;
        _problemDetails = problemDetails;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            AppException app => (app.StatusCode, app.Title, app.Message),
            DbUpdateException => (StatusCodes.Status409Conflict, "Конфлікт даних",
                "Не вдалося зберегти зміни через конфлікт даних (наприклад, порушення унікальності)."),
            OperationCanceledException => (499, "Запит скасовано", "Клієнт скасував запит."),
            _ => (StatusCodes.Status500InternalServerError, "Внутрішня помилка сервера",
                "Сталася непередбачена помилка. Спробуйте пізніше.")
        };

        if (status >= 500)
            _logger.LogError(exception, "Необроблений виняток при {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        else
            _logger.LogWarning("Прикладна помилка {Status}: {Message}", status, exception.Message);

        httpContext.Response.StatusCode = status;

        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            }
        });
    }
}
