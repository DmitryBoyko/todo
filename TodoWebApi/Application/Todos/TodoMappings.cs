namespace TodoWebApi.Application.Todos
{
    using TodoWebApi.Domain;

    public static class TodoMappings
    {
        public static TodoResponse ToResponse(this TodoItem entity) =>
            new(
                entity.Id,
                entity.Title,
                entity.Description,
                entity.IsCompleted,
                entity.Priority,
                entity.CreatedAt,
                entity.CompletedAt
            );
    }
}
