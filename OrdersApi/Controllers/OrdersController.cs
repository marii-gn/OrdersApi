using Microsoft.AspNetCore.Mvc;
using OrdersApi.Dtos;
using OrdersApi.Services;

namespace OrdersApi.Controllers;

/// <summary>CRUD-операції над замовленнями.</summary>
[ApiController]
[Route("api/orders")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders) => _orders = orders;

    /// <summary>Список замовлень з фільтрацією, пошуком, сортуванням і пагінацією.</summary>
    /// <remarks>
    /// Приклад: GET /api/orders?status=Paid&amp;minTotal=500&amp;search=кава&amp;sortBy=total&amp;desc=true
    ///
    /// sortBy: date | total | status | customer
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetAll([FromQuery] OrderQueryParameters query, CancellationToken ct)
        => Ok(await _orders.GetAllAsync(query, ct));

    /// <summary>Замовлення за ідентифікатором (разом з позиціями).</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetById(int id, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(id, ct);
        return order is null ? OrderNotFound(id) : Ok(order);
    }

    /// <summary>Створити замовлення. Сума, дата і статус визначаються сервером.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto dto, CancellationToken ct)
    {
        var created = await _orders.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Оновити замовлення (статус і позиції).</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderDto>> Update(int id, [FromBody] UpdateOrderDto dto, CancellationToken ct)
    {
        var updated = await _orders.UpdateAsync(id, dto, ct);
        return updated is null ? OrderNotFound(id) : Ok(updated);
    }

    /// <summary>Видалити замовлення.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await _orders.DeleteAsync(id, ct) ? NoContent() : OrderNotFound(id);

    private ObjectResult OrderNotFound(int id) =>
        Problem(statusCode: StatusCodes.Status404NotFound, title: "Ресурс не знайдено",
                detail: $"Замовлення з id={id} не знайдено.");
}
