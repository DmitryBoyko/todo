using Microsoft.EntityFrameworkCore;
using TodoWebApi.Application.Todos;
using TodoWebApi.Domain;

namespace TodoWebApi.Infrastructure.Repositories
{
    public interface ITodoRepository
    {
        Task<PagedResult<TodoItem>> GetAsync(TodoListQuery query, CancellationToken ct);
        Task<TodoItem?> GetByIdAsync(int id, CancellationToken ct);
        Task AddAsync(TodoItem item, CancellationToken ct);
        Task DeleteAsync(TodoItem item, CancellationToken ct);
        Task<int> SaveChangesAsync(CancellationToken ct);
    }

    public class TodoRepository : ITodoRepository
    {
        private readonly TodoDbContext _db;

        public TodoRepository(TodoDbContext db) => _db = db;

        public async Task<PagedResult<TodoItem>> GetAsync(TodoListQuery query, CancellationToken ct)
        {
            IQueryable<TodoItem> q = _db.Todos.AsNoTracking();

            if (query.IsCompleted.HasValue)
                q = q.Where(t => t.IsCompleted == query.IsCompleted.Value);

            if (query.Priority.HasValue)
                q = q.Where(t => t.Priority == query.Priority.Value);

            var total = await q.CountAsync(ct);

            var items = await q
                .OrderByDescending(t => t.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(ct);

            return new PagedResult<TodoItem>(items, query.Page, query.PageSize, total);
        }

        public Task<TodoItem?> GetByIdAsync(int id, CancellationToken ct) =>
            _db.Todos.FirstOrDefaultAsync(t => t.Id == id, ct);

        public Task AddAsync(TodoItem item, CancellationToken ct)
        {
            _db.Todos.Add(item);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(TodoItem item, CancellationToken ct)
        {
            _db.Todos.Remove(item);
            return Task.CompletedTask;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct) =>
            _db.SaveChangesAsync(ct);
    }
}
