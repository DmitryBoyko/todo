using TodoWebApi.Application.Todos;
using TodoWebApi.Domain;

namespace TodoWebApi.Infrastructure.Repositories
{
    public interface ITodoService
    {
        Task<PagedResult<TodoResponse>> GetAsync(TodoListQuery query, CancellationToken ct);
        Task<TodoResponse?> GetByIdAsync(int id, CancellationToken ct);
        Task<TodoResponse> CreateAsync(TodoCreateRequest request, CancellationToken ct);
        Task<TodoResponse?> UpdateAsync(int id, TodoUpdateRequest request, CancellationToken ct);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
        Task<TodoResponse?> CompleteAsync(int id, CancellationToken ct);
    }

    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _repo;

        public TodoService(ITodoRepository repo) => _repo = repo;

        public async Task<PagedResult<TodoResponse>> GetAsync(TodoListQuery query, CancellationToken ct)
        {
            var result = await _repo.GetAsync(query, ct);
            return new PagedResult<TodoResponse>(
                result.Items.Select(i => i.ToResponse()).ToList(),
                result.Page,
                result.PageSize,
                result.TotalCount
            );
        }

        public async Task<TodoResponse?> GetByIdAsync(int id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            return entity?.ToResponse();
        }

        public async Task<TodoResponse> CreateAsync(TodoCreateRequest request, CancellationToken ct)
        {
            var now = DateTime.UtcNow;

            var entity = new TodoItem
            {
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                IsCompleted = false,
                CreatedAt = now,
                CompletedAt = null
            };

            await _repo.AddAsync(entity, ct);
            await _repo.SaveChangesAsync(ct);

            return entity.ToResponse();
        }

        public async Task<TodoResponse?> UpdateAsync(int id, TodoUpdateRequest request, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) return null;

            entity.Title = request.Title;
            entity.Description = request.Description;
            entity.Priority = request.Priority;

            if (!entity.IsCompleted && request.IsCompleted)
            {
                entity.IsCompleted = true;
                entity.CompletedAt = DateTime.UtcNow;
            }
            else if (entity.IsCompleted && !request.IsCompleted)
            {
                entity.IsCompleted = false;
                entity.CompletedAt = null;
            }

            await _repo.SaveChangesAsync(ct);
            return entity.ToResponse();
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) return false;

            await _repo.DeleteAsync(entity, ct);
            await _repo.SaveChangesAsync(ct);
            return true;
        }

        public async Task<TodoResponse?> CompleteAsync(int id, CancellationToken ct)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity is null) return null;

            if (!entity.IsCompleted)
            {
                entity.IsCompleted = true;
                entity.CompletedAt = DateTime.UtcNow;
                await _repo.SaveChangesAsync(ct);
            }

            return entity.ToResponse();
        }
    }
}
