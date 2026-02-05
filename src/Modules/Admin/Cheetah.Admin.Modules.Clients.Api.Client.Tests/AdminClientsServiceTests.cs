using System.Net;
using System.Text.Json;
using Cheetah.Admin.Modules.Clients.Contracts.Requests;
using Cheetah.Admin.Modules.Clients.Contracts.Response;
using Cheetah.Contracts.Responses;
using Shouldly;

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
        var clients = new List<ClientViewModel>
        {
            new(Guid.NewGuid(), "Client 1", "Description 1", new TenantViewModel(Guid.NewGuid(), "Tenant 1")),
            new(Guid.NewGuid(), "Client 2", "Description 2", new TenantViewModel(Guid.NewGuid(), "Tenant 2"))
        };
        var gridResult = new GridResult<ClientViewModel>(clients, 2);

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(gridResult));
        var service = CreateService(handler);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Data.Count().ShouldBe(2);
        result.Total.ShouldBe(2);
        handler.RequestUri!.PathAndQuery.ShouldStartWith($"/{BasePath}");
        handler.Method.ShouldBe(HttpMethod.Get);
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ShouldReturnEmptyResult()
    {
        // Arrange
        var gridResult = new GridResult<ClientViewModel>(new List<ClientViewModel>(), 0);
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(gridResult));
        var service = CreateService(handler);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Data.ShouldBeEmpty();
        result.Total.ShouldBe(0);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WithExistingClient_ShouldReturnClient()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var expectedClient = new ClientViewModel(clientId, "Test Client", "Test Description", new TenantViewModel(Guid.NewGuid(), "Tenant"));

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(expectedClient));
        var service = CreateService(handler);

        // Act
        var result = await service.GetByIdAsync(clientId);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(clientId);
        handler.RequestUri!.PathAndQuery.ShouldBe($"/{BasePath}/{clientId}");
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
        result.ShouldBeNull();
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
        result.ShouldBe(expectedId);
        handler.RequestUri!.PathAndQuery.ShouldBe($"/{BasePath}");
        handler.Method.ShouldBe(HttpMethod.Post);
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
        await Should.ThrowAsync<HttpRequestException>(act);
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
        handler.RequestUri!.PathAndQuery.ShouldBe($"/{BasePath}/{clientId}");
        handler.Method.ShouldBe(HttpMethod.Put);
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
        await Should.ThrowAsync<HttpRequestException>(act);
    }

    #endregion

    #region GetAllTariffsAsync

    [Fact]
    public async Task GetAllTariffsAsync_ShouldReturnTariffs()
    {
        // Arrange
        var tariffs = new List<TariffViewModel>
        {
            new(Guid.NewGuid(), "Basic", "Basic plan", 9.99m, "USD", true),
            new(Guid.NewGuid(), "Premium", "Premium plan", 29.99m, "EUR", true)
        };
        var gridResult = new GridResult<TariffViewModel>(tariffs, 2);

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(gridResult));
        var service = CreateService(handler);

        // Act
        var result = await service.GetAllTariffsAsync();

        // Assert
        result.Data.Count().ShouldBe(2);
        result.Total.ShouldBe(2);
        handler.RequestUri!.PathAndQuery.ShouldStartWith("/api/tariffs");
        handler.Method.ShouldBe(HttpMethod.Get);
    }

    [Fact]
    public async Task GetAllTariffsAsync_WhenEmpty_ShouldReturnEmptyResult()
    {
        // Arrange
        var gridResult = new GridResult<TariffViewModel>(new List<TariffViewModel>(), 0);
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(gridResult));
        var service = CreateService(handler);

        // Act
        var result = await service.GetAllTariffsAsync();

        // Assert
        result.Data.ShouldBeEmpty();
        result.Total.ShouldBe(0);
    }

    #endregion

    #region GetTariffByIdAsync

    [Fact]
    public async Task GetTariffByIdAsync_WithExistingTariff_ShouldReturnTariff()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var expectedTariff = new TariffViewModel(tariffId, "Basic", "Basic plan", 9.99m, "USD", true);

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(expectedTariff));
        var service = CreateService(handler);

        // Act
        var result = await service.GetTariffByIdAsync(tariffId);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(tariffId);
        handler.RequestUri!.PathAndQuery.ShouldBe($"/api/tariffs/{tariffId}");
    }

    [Fact]
    public async Task GetTariffByIdAsync_WithNonExistingTariff_ShouldReturnNull()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "");
        var service = CreateService(handler);

        // Act
        var result = await service.GetTariffByIdAsync(tariffId);

        // Assert
        result.ShouldBeNull();
    }

    #endregion

    #region CreateTariffAsync

    [Fact]
    public async Task CreateTariffAsync_WithValidRequest_ShouldReturnNewId()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var request = new CreateTariffRequest("New Plan", "Description", 19.99m, "USD", true);
        var response = new { Id = expectedId };

        var handler = new MockHttpMessageHandler(HttpStatusCode.Created, JsonSerializer.Serialize(response));
        var service = CreateService(handler);

        // Act
        var result = await service.CreateTariffAsync(request);

        // Assert
        result.ShouldBe(expectedId);
        handler.RequestUri!.PathAndQuery.ShouldBe("/api/tariffs");
        handler.Method.ShouldBe(HttpMethod.Post);
    }

    #endregion

    #region UpdateTariffAsync

    [Fact]
    public async Task UpdateTariffAsync_WithValidRequest_ShouldSucceed()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var request = new UpdateTariffRequest(tariffId, "Updated Plan", "Updated Description", 29.99m, "EUR", true);
        var handler = new MockHttpMessageHandler(HttpStatusCode.NoContent, "");
        var service = CreateService(handler);

        // Act
        await service.UpdateTariffAsync(tariffId, request);

        // Assert
        handler.RequestUri!.PathAndQuery.ShouldBe($"/api/tariffs/{tariffId}");
        handler.Method.ShouldBe(HttpMethod.Put);
    }

    #endregion

    #region DeleteTariffAsync

    [Fact]
    public async Task DeleteTariffAsync_WithExistingTariff_ShouldSucceed()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var handler = new MockHttpMessageHandler(HttpStatusCode.NoContent, "");
        var service = CreateService(handler);

        // Act
        await service.DeleteTariffAsync(tariffId);

        // Assert
        handler.RequestUri!.PathAndQuery.ShouldBe($"/api/tariffs/{tariffId}");
        handler.Method.ShouldBe(HttpMethod.Delete);
    }

    [Fact]
    public async Task DeleteTariffAsync_WithNonExistingTariff_ShouldThrow()
    {
        // Arrange
        var tariffId = Guid.NewGuid();
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "");
        var service = CreateService(handler);

        // Act
        var act = () => service.DeleteTariffAsync(tariffId).AsTask();

        // Assert
        await Should.ThrowAsync<HttpRequestException>(act);
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
