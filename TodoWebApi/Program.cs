using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using TodoWebApi.Api.Middleware;
using TodoWebApi.Infrastructure;
using TodoWebApi.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ---------- Инфраструктура: БД и DI ----------

// InMemory DbContext
builder.Services.AddDbContext<TodoDbContext>(opt =>
    opt.UseInMemoryDatabase("TodoDb"));

// Репозиторий и сервисы
builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ITodoService, TodoService>();

// ---------- Контроллеры + ProblemDetails ----------

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Единый формат ошибок валидации (400)
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Type = "https://httpstatuses.com/400",
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.HttpContext.Request.Path
            };

            return new BadRequestObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" }
            };
        };
    });

// ProblemDetails для остальных ошибок
builder.Services.AddProblemDetails();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------- Rate limiting (на весь API) ----------

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10);
        opt.PermitLimit = 100; // 100 запросов за окно
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

// Логирование
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// ---------- Middleware-цепочка ----------

// Глобальный middleware ошибок (использует ProblemDetails внутри)
app.UseGlobalExceptionHandling();

// Rate limiting на все запросы к API
app.UseRateLimiter();

// Swagger только в Dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Маршрутизация контроллеров
app.MapControllers()
   .RequireRateLimiting("api"); // применяем политику rate limiting к контроллерам

app.Run();

// Для WebApplicationFactory в тестах
public partial class Program { }
