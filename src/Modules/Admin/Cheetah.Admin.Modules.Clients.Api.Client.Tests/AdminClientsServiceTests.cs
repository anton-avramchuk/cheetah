using System.Net;
using System.Text.Json;
using Cheetah.Admin.Modules.Clients.Contracts.Requests;
using Cheetah.Admin.Modules.Clients.Contracts.Response;
using FluentAssertions;

namespace Cheetah.Admin.Modules.Clients.Api.Client.Tests;

public class AdminClientsServiceTests
{
    private const string BasePath = "api/clients";

    private static AdminClientsService CreateService(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        return new AdminClientsService(httpClient);
    }

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ShouldReturnClients()
    {
        // Arrange
        var expectedClients = new List<ClientViewModel>
        {
            new(Guid.NewGuid(), "Client 1", new TenantViewModel(Guid.NewGuid(), "Tenant 1")),
            new(Guid.NewGuid(), "Client 2", new TenantViewModel(Guid.NewGuid(), "Tenant 2"))
        };

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(expectedClients));
        var service = CreateService(handler);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        handler.RequestUri!.PathAndQuery.Should().Be($"/{BasePath}");
        handler.Method.Should().Be(HttpMethod.Get);
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "[]");
        var service = CreateService(handler);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WithExistingClient_ShouldReturnClient()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var expectedClient = new ClientViewModel(clientId, "Test Client", new TenantViewModel(Guid.NewGuid(), "Tenant"));

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(expectedClient));
        var service = CreateService(handler);

        // Act
        var result = await service.GetByIdAsync(clientId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(clientId);
        handler.RequestUri!.PathAndQuery.Should().Be($"/{BasePath}/{clientId}");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingClient_ShouldReturnNull()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "");
        var service = CreateService(handler);

        // Act
        var result = await service.GetByIdAsync(clientId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldReturnNewId()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var request = new CreateClientRequest("New Client", "Description");
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
    public async Task CreateAsync_WithServerError_ShouldThrow()
    {
        // Arrange
        var request = new CreateClientRequest("New Client", "Description");
        var handler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, "");
        var service = CreateService(handler);

        // Act
        var act = () => service.CreateAsync(request).AsTask();

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new UpdateClientRequest(clientId, "Updated Name", "Updated Description");
        var handler = new MockHttpMessageHandler(HttpStatusCode.NoContent, "");
        var service = CreateService(handler);

        // Act
        await service.UpdateAsync(clientId, request);

        // Assert
        handler.RequestUri!.PathAndQuery.Should().Be($"/{BasePath}/{clientId}");
        handler.Method.Should().Be(HttpMethod.Put);
    }

    [Fact]
    public async Task UpdateAsync_WithNotFound_ShouldThrow()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = new UpdateClientRequest(clientId, "Updated Name", "Updated Description");
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "");
        var service = CreateService(handler);

        // Act
        var act = () => service.UpdateAsync(clientId, request).AsTask();

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    #endregion

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

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
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
