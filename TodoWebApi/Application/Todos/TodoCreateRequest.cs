using System.ComponentModel.DataAnnotations;
using TodoWebApi.Domain;

namespace TodoWebApi.Application.Todos
{
    public record TodoCreateRequest(
     [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    string Title,

     [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    string? Description,

     [Required(ErrorMessage = "Priority is required.")]
    TodoPriority Priority
 );
}
