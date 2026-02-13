using TodoWebApi.Domain;

namespace TodoWebApi.Application.Todos
{
    public record TodoResponse(
      int Id,
      string Title,
      string? Description,
      bool IsCompleted,
      TodoPriority Priority,
      DateTime CreatedAt,
      DateTime? CompletedAt
  );
}
