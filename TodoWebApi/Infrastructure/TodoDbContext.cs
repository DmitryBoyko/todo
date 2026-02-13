using Microsoft.EntityFrameworkCore;
using TodoWebApi.Domain;

namespace TodoWebApi.Infrastructure
{
    public class TodoDbContext : DbContext
    {
        public DbSet<TodoItem> Todos => Set<TodoItem>();

        public TodoDbContext(DbContextOptions<TodoDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var todo = modelBuilder.Entity<TodoItem>();

            todo.HasKey(x => x.Id);

            todo.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            todo.Property(x => x.Description)
                .HasMaxLength(1000);

            todo.Property(x => x.Priority)
                .HasConversion<int>();

            todo.Property(x => x.IsCompleted)
                .HasDefaultValue(false);

            todo.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
