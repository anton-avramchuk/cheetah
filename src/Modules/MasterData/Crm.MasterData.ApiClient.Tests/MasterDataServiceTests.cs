using System.Net;
using System.Text.Json;
using Crm.MasterData.Contracts.Requests;
using Crm.MasterData.Contracts.Response;
using Shouldly;

namespace Crm.MasterData.ApiClient.Tests;

public class MasterDataServiceTests
{
    private const string BasePath = "api/masterdata";

    private static MasterDataService CreateService(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        return new MasterDataService(httpClient);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEntities()
    {
        // Arrange
        var expectedEntities = new List<StackItemViewModel>
        {
            new(Guid.NewGuid(), "Entity 1", "Description 1"),
            new(Guid.NewGuid(), "Entity 2", "Description 2")
        };

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(expectedEntities));
        var service = CreateService(handler);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Count.ShouldBe(2);
        handler.RequestUri!.PathAndQuery.ShouldBe($"/{BasePath}");
        handler.Method.ShouldBe(HttpMethod.Get);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingEntity_ShouldReturnEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var expectedEntity = new StackItemViewModel(entityId, "Test Entity", "Description");

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
        var request = new CreateStackItemRequest("New Entity", "Description");
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