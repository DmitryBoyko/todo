namespace TodoWebApi.Domain
{
    public class TodoItem
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsCompleted { get; set; } = false;

        public TodoPriority Priority { get; set; } = TodoPriority.Low;

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
