using System.Net;
using System.Text.Json;
using Cheetah.Contracts.Responses;
using Crm.VacancyTasks.Contracts.Requests;
using Crm.VacancyTasks.Contracts.Response;
using Shouldly;

namespace Crm.VacancyTasks.ApiClient.Tests;

public class VacancyTasksServiceTests
{
    private const string BasePath = "api/vacancy-tasks";

    private static VacancyTasksService CreateService(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        return new VacancyTasksService(httpClient);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEntities()
    {
        // Arrange
        var vacancyId = Guid.NewGuid();
        var stateId = Guid.NewGuid();
        var expectedEntities = new List<VacancyTaskViewModel>
        {
            new(Guid.NewGuid(), "Entity 1", "Description 1", vacancyId, stateId, null, null, null, 0),
            new(Guid.NewGuid(), "Entity 2", "Description 2", vacancyId, stateId, null, null, null, 1)
        };

        var gridResult = new GridResult<VacancyTaskViewModel>(expectedEntities, expectedEntities.Count);
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(gridResult));
        var service = CreateService(handler);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Total.ShouldBe(2);
        handler.RequestUri!.PathAndQuery.ShouldStartWith($"/{BasePath}");
        handler.Method.ShouldBe(HttpMethod.Get);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingEntity_ShouldReturnEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var vacancyId = Guid.NewGuid();
        var stateId = Guid.NewGuid();
        var expectedEntity = new VacancyTaskViewModel(entityId, "Test Entity", "Description", vacancyId, stateId, null, null, null, 0);

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(expectedEntity));
        var service = CreateService(handler);

        // Act
        var result = await service.GetByIdAsync(entityId);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(entityId);
        handler.RequestUri!.PathAndQuery.ShouldBe($"/{BasePath}/{entityId}");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingEntity_ShouldReturnNull()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "");
        var service = CreateService(handler);

        // Act
        var result = await service.GetByIdAsync(entityId);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldReturnNewId()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var vacancyId = Guid.NewGuid();
        var stateId = Guid.NewGuid();
        var request = new CreateVacancyTaskRequest("New Entity", vacancyId, stateId, "Description", null, null, null);
        var response = new { Id = expectedId };

        var handler = new MockHttpMessageHandler(HttpStatusCode.Created, JsonSerializer.Serialize(response));
        var service = CreateService(handler);

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        result.ShouldBe(expectedId);
        handler.RequestUri!.PathAndQuery.ShouldBe($"/{BasePath}");
        handler.Method.ShouldBe(HttpMethod.Post);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingEntity_ShouldSucceed()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var handler = new MockHttpMessageHandler(HttpStatusCode.NoContent, "");
        var service = CreateService(handler);

        // Act
        await service.DeleteAsync(entityId);

        // Assert
        handler.RequestUri!.PathAndQuery.ShouldBe($"/{BasePath}/{entityId}");
        handler.Method.ShouldBe(HttpMethod.Delete);
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        public Uri? RequestUri { get; private set; }
        public HttpMethod? Method { get; private set; }

        public MockHttpMessageHandler(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            Method = request.Method;

            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content, System.Text.Encoding.UTF8, "application/json")
            });
        }
    }
}
