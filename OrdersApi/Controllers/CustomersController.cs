using Microsoft.AspNetCore.Mvc;
using OrdersApi.Dtos;
using OrdersApi.Services;

namespace OrdersApi.Controllers;

/// <summary>CRUD-операції над клієнтами.</summary>
[ApiController]
[Route("api/customers")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customers;
    private readonly IOrderService _orders;

    public CustomersController(ICustomerService customers, IOrderService orders)
    {
        _customers = customers;
        _orders = orders;
    }

    /// <summary>Список клієнтів з пошуком, фільтрацією, сортуванням і пагінацією.</summary>
    /// <remarks>sortBy: name | email | createdAt | ordersCount</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetAll([FromQuery] CustomerQueryParameters query, CancellationToken ct)
        => Ok(await _customers.GetAllAsync(query, ct));

    /// <summary>Клієнт за ідентифікатором.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetById(int id, CancellationToken ct)
    {
        var customer = await _customers.GetByIdAsync(id, ct);
        return customer is null ? CustomerNotFound(id) : Ok(customer);
    }

    /// <summary>Замовлення конкретного клієнта (вкладений ресурс).</summary>
    [HttpGet("{id:int}/orders")]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetOrders(int id, [FromQuery] OrderQueryParameters query, CancellationToken ct)
    {
        if (await _customers.GetByIdAsync(id, ct) is null)
            return CustomerNotFound(id);

        query.CustomerId = id;
        return Ok(await _orders.GetAllAsync(query, ct));
    }

    /// <summary>Створити клієнта.</summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerDto dto, CancellationToken ct)
    {
        var created = await _customers.CreateAsync(dto, ct);
        // 201 Created + заголовок Location: /api/customers/{id}
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Оновити клієнта.</summary>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerDto>> Update(int id, [FromBody] UpdateCustomerDto dto, CancellationToken ct)
    {
        var updated = await _customers.UpdateAsync(id, dto, ct);
        return updated is null ? CustomerNotFound(id) : Ok(updated);
    }

    /// <summary>Видалити клієнта (разом з його замовленнями).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await _customers.DeleteAsync(id, ct) ? NoContent() : CustomerNotFound(id);

    private ObjectResult CustomerNotFound(int id) =>
        Problem(statusCode: StatusCodes.Status404NotFound, title: "Ресурс не знайдено",
                detail: $"Клієнта з id={id} не знайдено.");
}
