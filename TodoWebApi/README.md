# TodoWebApi (.NET 9, ASP.NET Core, EF Core InMemory)

REST API для управления списком задач (Todo).  
Стек: **.NET 9**, **ASP.NET Core Web API (Controllers)**, **Entity Framework Core InMemory**, **Swagger / OpenAPI**, **ProblemDetails**, **Rate Limiting**, интеграционные тесты (xUnit + WebApplicationFactory).

## Возможности

- CRUD для сущности `TodoItem`:
  - `GET /api/todos` — список задач с фильтрацией и пагинацией
  - `GET /api/todos/{id}` — получить задачу по идентификатору
  - `POST /api/todos` — создать задачу
  - `PUT /api/todos/{id}` — полное обновление задачи
  - `DELETE /api/todos/{id}` — удаление задачи
  - `PATCH /api/todos/{id}/complete` — отметить задачу выполненной
- Фильтрация:
  - `isCompleted` (bool?)
  - `priority` (Low | Medium | High)
- Пагинация:
  - `page` (int, > 0)
  - `pageSize` (int, 1–1000)
- Валидация входных данных (DataAnnotations) и возврат ошибок в формате ProblemDetails.
- Глобальная обработка исключений через middleware.
- EF Core InMemory база данных.
- Swagger UI для интерактивного тестирования.
- Базовый rate limiting для API.
- Интеграционные тесты (xUnit + WebApplicationFactory) для основных сценариев.

## Структура проекта

Проект реализован как один Web API‑проект с логическим разделением по папкам:

```text
TodoWebApi/
  Api/
    Controllers/
      TodosController.cs            # REST-контроллер для работы с Todo
    Middleware/
      ExceptionHandlingMiddleware.cs# Глобальный обработчик исключений (ProblemDetails)
  Application/
    Todos/
      PagedResult.cs
      TodoCreateRequest.cs
      TodoListQuery.cs
      TodoMappings.cs
      TodoResponse.cs
      TodoUpdateRequest.cs         # DTO и маппинги уровня приложения
  Domain/
    TodoItem.cs                     # Доменная модель задачи
    TodoPriority.cs                 # enum приоритета (Low, Medium, High)
  Infrastructure/
    TodoDbContext.cs                # EF Core InMemory DbContext
    Repositories/
      ITodoRepository.cs
      TodoRepository.cs
      ITodoService.cs              # Контракты и реализации доступа к данным/бизнес-логики
      TodoService.cs
  Program.cs                        # Точка входа, DI, middleware, маршрутизация
  appsettings.json
  Dockerfile (опционально)

TodoTestProject/
  TodoApiFactory.cs                 # WebApplicationFactory<Program> для интеграционных тестов
  TodoEndpointsTests.cs             # Тесты: POST/GET/PUT/DELETE/PATCH
README.md
TODO.md
