using System.Net;
using System.Text.Json;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;
using FluentAssertions;

namespace Crm.Recruitment.ApiClient.Tests;

public class RecruitmentServiceTests
{
    private const string BasePath = "api/recruitment";

    private static RecruitmentService CreateService(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        return new RecruitmentService(httpClient);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEntities()
    {
        // Arrange
        var expectedEntities = new List<VacancyViewModel>
        {
            new(Guid.NewGuid(), "Entity 1", "Description 1"),
            new(Guid.NewGuid(), "Entity 2", "Description 2")
        };

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(expectedEntities));
        var service = CreateService(handler);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        handler.RequestUri!.PathAndQuery.Should().Be($"/{BasePath}");
        handler.Method.Should().Be(HttpMethod.Get);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingEntity_ShouldReturnEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var expectedEntity = new VacancyViewModel(entityId, "Test Entity", "Description");

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(expectedEntity));
        var service = CreateService(handler);

        // Act
        var result = await service.GetByIdAsync(entityId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entityId);
        handler.RequestUri!.PathAndQuery.Should().Be($"/{BasePath}/{entityId}");
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
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldReturnNewId()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var request = new CreateVacancyRequest("New Entity", "Description");
        var response = new { Id = expectedId };

        var handler = new MockHttpMessageHandler(HttpStatusCode.Created, JsonSerializer.Serialize(response));
        var service = CreateService(handler);

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        result.Should().Be(expectedId);
        handler.RequestUri!.PathAndQuery.Should().Be($"/{BasePath}");
        handler.Method.Should().Be(HttpMethod.Post);
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
        handler.RequestUri!.PathAndQuery.Should().Be($"/{BasePath}/{entityId}");
        handler.Method.Should().Be(HttpMethod.Delete);
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