namespace TodoWebApi.Application.Todos
{
    public record PagedResult<T>(
     IReadOnlyCollection<T> Items,
     int Page,
     int PageSize,
     int TotalCount
 );
}
