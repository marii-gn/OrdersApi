using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrdersApi.Data;
using OrdersApi.Middleware;
using OrdersApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- База даних (EF Core) ----------
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var provider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
    if (provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql"));
    else
        options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});

// ---------- Сервісний шар (Dependency Injection) ----------
// Scoped: один екземпляр на HTTP-запит — так само, як і DbContext
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// ---------- Контролери + JSON ----------
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // enum у JSON як рядок: "Paid", а не 1
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// ---------- Обробка помилок у форматі ProblemDetails (RFC 7807) ----------
builder.Services.AddProblemDetails(o =>
{
    o.CustomizeProblemDetails = ctx =>
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ---------- OpenAPI / Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Orders API",
        Version = "v1",
        Description = "REST API для керування клієнтами та замовленнями (ЛР2, ASP.NET Core)"
    });
    var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlPath))
        o.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

await DbInitializer.InitializeAsync(app.Services);

// ---------- HTTP pipeline ----------
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o => o.DocumentTitle = "Orders API");
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
