using System.Net;
using System.Text.Json;
using Cheetah.Contracts.Responses;
using Crm.Candidates.Contracts.Requests;
using Crm.Candidates.Contracts.Response;
using Shouldly;

namespace Crm.Candidates.ApiClient.Tests;

public class CandidatesServiceTests
{
    private const string BasePath = "api/candidates";

    private static CandidatesService CreateService(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        return new CandidatesService(httpClient);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEntities()
    {
        // Arrange
        var expectedEntities = new List<CandidateViewModel>
        {
            new(Guid.NewGuid(), "John", "Doe", null, null, null, null, null, null, null),
            new(Guid.NewGuid(), "Jane", "Smith", null, null, null, null, null, null, null)
        };

        var gridResult = new GridResult<CandidateViewModel>(expectedEntities, expectedEntities.Count);
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
        var expectedEntity = new CandidateViewModel(entityId, "John", "Doe", "john@test.com", null, null, null, null, null, null);

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
        var request = new CreateCandidateRequest("John", "Doe", null, null, null, null, null, null, null);
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
