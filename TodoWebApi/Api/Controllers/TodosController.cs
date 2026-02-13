using Microsoft.AspNetCore.Mvc;
using TodoWebApi.Application.Todos;
using TodoWebApi.Domain;
using TodoWebApi.Infrastructure.Repositories;

namespace TodoWebApi.Api.Controllers
{
    // REST-контроллер для управления Todo-задачами: CRUD, фильтрация и завершение задач.
    [ApiController]
    [Route("api/[controller]")]
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _service;
        private readonly ILogger<TodosController> _logger;

        public TodosController(ITodoService service, ILogger<TodosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<TodoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTodos(
            [FromQuery] bool? isCompleted,
            [FromQuery] TodoPriority? priority,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            if (page <= 0 || pageSize <= 0 || pageSize > 1000)
            {
                return BadRequest(new ProblemDetails
                {
                    Type = "https://httpstatuses.com/400",
                    Title = "Invalid pagination parameters.",
                    Detail = "Page and pageSize must be positive. pageSize must be <= 1000.",
                    Status = StatusCodes.Status400BadRequest,
                    Instance = HttpContext.Request.Path
                });
            }

            var query = new TodoListQuery(isCompleted, priority, page, pageSize);
            var result = await _service.GetAsync(query, ct);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TodoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTodo(int id, CancellationToken ct = default)
        {
            var todo = await _service.GetByIdAsync(id, ct);
            if (todo is null)
            {
                return NotFound(new ProblemDetails
                {
                    Type = "https://httpstatuses.com/404",
                    Title = "Todo item not found.",
                    Detail = $"Todo item with id '{id}' was not found.",
                    Status = StatusCodes.Status404NotFound,
                    Instance = HttpContext.Request.Path
                });
            }

            return Ok(todo);
        }

        [HttpPost]
        [ProducesResponseType(typeof(TodoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTodo(
            [FromBody] TodoCreateRequest request,
            CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var created = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetTodo), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(TodoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTodo(
            int id,
            [FromBody] TodoUpdateRequest request,
            CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var updated = await _service.UpdateAsync(id, request, ct);
            if (updated is null)
            {
                return NotFound(new ProblemDetails
                {
                    Type = "https://httpstatuses.com/404",
                    Title = "Todo item not found.",
                    Detail = $"Todo item with id '{id}' was not found.",
                    Status = StatusCodes.Status404NotFound,
                    Instance = HttpContext.Request.Path
                });
            }

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTodo(int id, CancellationToken ct = default)
        {
            var removed = await _service.DeleteAsync(id, ct);
            if (!removed)
            {
                return NotFound(new ProblemDetails
                {
                    Type = "https://httpstatuses.com/404",
                    Title = "Todo item not found.",
                    Detail = $"Todo item with id '{id}' was not found.",
                    Status = StatusCodes.Status404NotFound,
                    Instance = HttpContext.Request.Path
                });
            }

            return NoContent();
        }

        [HttpPatch("{id:int}/complete")]
        [ProducesResponseType(typeof(TodoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CompleteTodo(int id, CancellationToken ct = default)
        {
            var completed = await _service.CompleteAsync(id, ct);
            if (completed is null)
            {
                return NotFound(new ProblemDetails
                {
                    Type = "https://httpstatuses.com/404",
                    Title = "Todo item not found.",
                    Detail = $"Todo item with id '{id}' was not found.",
                    Status = StatusCodes.Status404NotFound,
                    Instance = HttpContext.Request.Path
                });
            }

            return Ok(completed);
        }
    }

}
