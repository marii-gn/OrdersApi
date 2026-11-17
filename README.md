# ЛР2 — Orders API (ASP.NET Core REST API)

Предметна область: інтернет-магазин — **клієнти** (`Customer`) → **замовлення** (`Order`) → **позиції замовлення** (`OrderItem`).
Продовження моделі даних з ЛР1.

## Запуск

```bash
cd OrdersApi
dotnet run --launch-profile http
```

* Swagger UI: http://localhost:5080/swagger
* Тестові запити: файл `OrdersApi/OrdersApi.http` (Visual Studio → кнопка *Send request*)
* БД: SQLite-файл `orders_api.db` створюється автоматично з демо-даними. Щоб почати «з нуля» — видаліть файл.
* PostgreSQL: у `appsettings.json` поставте `"DatabaseProvider": "PostgreSql"` і перевірте рядок підключення.

## Endpoint'и

| Метод | Шлях | Опис | Коди |
|---|---|---|---|
| GET | /api/customers | список + search, minOrders, sortBy, desc, page, pageSize | 200, 400 |
| GET | /api/customers/{id} | клієнт за id | 200, 404 |
| GET | /api/customers/{id}/orders | замовлення клієнта | 200, 404 |
| POST | /api/customers | створити | 201, 400, 409 |
| PUT | /api/customers/{id} | оновити | 200, 400, 404, 409 |
| DELETE | /api/customers/{id} | видалити | 204, 404 |
| GET | /api/orders | список + customerId, status, minTotal, maxTotal, fromDate, toDate, search, sortBy, desc | 200, 400 |
| GET | /api/orders/{id} | замовлення за id | 200, 404 |
| POST | /api/orders | створити | 201, 400 |
| PUT | /api/orders/{id} | оновити | 200, 400, 404, 409 |
| DELETE | /api/orders/{id} | видалити | 204, 404 |
