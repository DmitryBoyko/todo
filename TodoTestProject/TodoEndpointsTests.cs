using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TodoWebApi.Application.Todos;
using TodoWebApi.Domain;

namespace TodoTestProject
{
    public class TodoEndpointsTests(TodoApiFactory factory) : IClassFixture<TodoApiFactory>
    {
        private readonly HttpClient _client = factory.CreateClient();
        
        [Fact]
        public async Task Create_And_Get_Todo_Success()
        {
            // Arrange
            var request = new TodoCreateRequest(
                Title: "Test todo",
                Description: "Desc",
                Priority: TodoPriority.Medium);

            // Act: создаём задачу
            var createResponse = await _client.PostAsJsonAsync("/api/todos", request);

            // Проверяем, что создание прошло успешно
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
            created.Should().NotBeNull();
            created!.Title.Should().Be("Test todo");

            // Act: читаем задачу по id
            var getResponse = await _client.GetAsync($"/api/todos/{created.Id}");

            // ВРЕМЕННО: логируем тело ответа, чтобы увидеть ошибку 500
            var body = await getResponse.Content.ReadAsStringAsync();
            Console.WriteLine("GET /api/todos/{id} response:");
            Console.WriteLine(body);

            // Assert
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var got = await getResponse.Content.ReadFromJsonAsync<TodoResponse>();
            got.Should().NotBeNull();
            got!.Id.Should().Be(created.Id);
        }

        [Fact]
        public async Task Get_NotFound_Returns_404_ProblemDetails()
        {
            var response = await _client.GetAsync("/api/todos/9999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var json = await response.Content.ReadAsStringAsync();
            json.Should().Contain("\"type\"");
            json.Should().Contain("\"title\"");
            json.Should().Contain("\"status\"");
        }

        [Fact]
        public async Task Update_Todo_With_Put_Success()
        {
            // Arrange: сначала создаём задачу
            var createRequest = new TodoCreateRequest(
                Title: "Original title",
                Description: "Original desc",
                Priority: TodoPriority.Low);

            var createResponse = await _client.PostAsJsonAsync("/api/todos", createRequest);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
            created.Should().NotBeNull();
            var id = created!.Id;

            // Act: обновляем
            var updateRequest = new TodoUpdateRequest(
                Title: "Updated title",
                Description: "Updated desc",
                IsCompleted: true,
                Priority: TodoPriority.High);

            var updateResponse = await _client.PutAsJsonAsync($"/api/todos/{id}", updateRequest);

            // Assert
            updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var updated = await updateResponse.Content.ReadFromJsonAsync<TodoResponse>();
            updated.Should().NotBeNull();
            updated!.Title.Should().Be("Updated title");
            updated.Description.Should().Be("Updated desc");
            updated.IsCompleted.Should().BeTrue();
            updated.Priority.Should().Be(TodoPriority.High);
            updated.CompletedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task Delete_Todo_Success_And_Then_NotFound()
        {
            // Arrange: создаём задачу
            var createRequest = new TodoCreateRequest(
                Title: "To delete",
                Description: null,
                Priority: TodoPriority.Medium);

            var createResponse = await _client.PostAsJsonAsync("/api/todos", createRequest);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
            created.Should().NotBeNull();
            var id = created!.Id;

            // Act: удаляем
            var deleteResponse = await _client.DeleteAsync($"/api/todos/{id}");

            // Assert: успешное удаление
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // И последующий GET возвращает 404
            var getResponse = await _client.GetAsync($"/api/todos/{id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Complete_Todo_With_Patch_Sets_IsCompleted_And_CompletedAt()
        {
            // Arrange: создаём невыполненную задачу
            var createRequest = new TodoCreateRequest(
                Title: "To complete",
                Description: null,
                Priority: TodoPriority.Low);

            var createResponse = await _client.PostAsJsonAsync("/api/todos", createRequest);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await createResponse.Content.ReadFromJsonAsync<TodoResponse>();
            created.Should().NotBeNull();
            var id = created!.Id;

            // Act: отмечаем как выполненную
            var patchResponse = await _client.PatchAsync($"/api/todos/{id}/complete", content: null);

            // Assert
            patchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var completed = await patchResponse.Content.ReadFromJsonAsync<TodoResponse>();
            completed.Should().NotBeNull();
            completed!.IsCompleted.Should().BeTrue();
            completed.CompletedAt.Should().NotBeNull();
        }
    }
}