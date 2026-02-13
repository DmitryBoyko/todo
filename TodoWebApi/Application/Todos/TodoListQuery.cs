using TodoWebApi.Domain;

namespace TodoWebApi.Application.Todos
{
    public record TodoListQuery(
     bool? IsCompleted,
     TodoPriority? Priority,
     int Page = 1,
     int PageSize = 10
 );
}
