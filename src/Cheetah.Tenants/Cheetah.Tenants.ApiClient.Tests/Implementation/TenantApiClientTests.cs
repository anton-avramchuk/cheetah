using System.Net;
using Cheetah.Tenants.ApiClient.Implementation;
using Cheetah.Tenants.ApiClient.Tests.TestHelpers;
using Cheetah.Tenants.Contracts.Requests;
using Cheetah.Tenants.Contracts.ViewModels;
using FluentAssertions;

namespace Cheetah.Tenants.ApiClient.Tests.Implementation;

public class TenantApiClientTests
{
    [Fact]
    public async Task GetAllAsync_WhenSuccessful_ShouldReturnTenantList()
    {
        // Arrange
        var expectedTenants = new List<TenantViewModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Tenant 1", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Tenant 2", IsActive = false }
        };

        var handler = new HttpMessageHandlerMock((request, ct) =>
        {
            request.Method.Should().Be(HttpMethod.Get);
            request.RequestUri!.ToString().Should().EndWith("/api/tenants");

            return Task.FromResult(HttpMessageHandlerMock.CreateJsonResponse(expectedTenants));
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new TenantApiClient(httpClient);

        // Act
        var result = await client.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(expectedTenants);
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        var handler = new HttpMessageHandlerMock((request, ct) =>
        {
            return Task.FromResult(HttpMessageHandlerMock.CreateJsonResponse(new List<TenantViewModel>()));
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new TenantApiClient(httpClient);

        // Act
        var result = await client.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenNullResponse_ShouldReturnEmptyList()
    {
        // Arrange
        var handler = new HttpMessageHandlerMock((request, ct) =>
        {
            return Task.FromResult(HttpMessageHandlerMock.CreateJsonResponse<List<TenantViewModel>?>(null));
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new TenantApiClient(httpClient);

        // Act
        var result = await client.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenServerError_ShouldThrowHttpRequestException()
    {
        // Arrange
        var handler = new HttpMessageHandlerMock((request, ct) =>
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new TenantApiClient(httpClient);

        // Act
        var act = async () => await client.GetAllAsync();

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenTenantExists_ShouldReturnTenant()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var expectedTenant = new TenantViewModel
        {
            Id = tenantId,
            Name = "Test Tenant",
            Subdomain = "test",
            IsActive = true
        };

        var handler = new HttpMessageHandlerMock((request, ct) =>
        {
            request.Method.Should().Be(HttpMethod.Get);
            request.RequestUri!.ToString().Should().EndWith($"/api/tenants/{tenantId}");

            return Task.FromResult(HttpMessageHandlerMock.CreateJsonResponse(expectedTenant));
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new TenantApiClient(httpClient);

        // Act
        var result = await client.GetByIdAsync(tenantId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedTenant);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTenantNotFound_ShouldReturnNull()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var handler = new HttpMessageHandlerMock((request, ct) =>
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new TenantApiClient(httpClient);

        // Act
        var result = await client.GetByIdAsync(tenantId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenServerError_ShouldThrowHttpRequestException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var handler = new HttpMessageHandlerMock((request, ct) =>
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new TenantApiClient(httpClient);

        // Act
        var act = async () => await client.GetByIdAsync(tenantId);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task CreateAsync_WhenSuccessful_ShouldReturnNewTenantId()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var request = new CreateTenantRequest
        {
            Name = "New Tenant",
            Subdomain = "new-tenant"
        };

        var handler = new HttpMessageHandlerMock(async (req, ct) =>
        {
            req.Method.Should().Be(HttpMethod.Post);
            req.RequestUri!.ToString().Should().EndWith("/api/tenants");

            var content = await req.Content!.ReadAsStringAsync(ct);
            content.Should().Contain("New Tenant");
            content.Should().Contain("new-tenant");

            return HttpMessageHandlerMock.CreateJsonResponse(expectedId);
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new TenantApiClient(httpClient);

        // Act
        var result = await client.CreateAsync(request);

        // Assert
        result.Should().Be(expectedId);
    }

    [Fact]
    public async Task CreateAsync_WhenServerError_ShouldThrowHttpRequestException()
    {
        // Arrange
        var request = new CreateTenantRequest
        {
            Name = "New Tenant",
            Subdomain = "new-tenant"
        };

        var handler = new HttpMessageHandlerMock((req, ct) =>
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new TenantApiClient(httpClient);

        // Act
        var act = async () => await client.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task ActivateAsync_WhenSuccessful_ShouldCompleteWithoutException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var handler = new HttpMessageHandlerMock((request, ct) =>
        {
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.ToString().Should().EndWith($"/api/tenants/{tenantId}/activate");

            return Task.FromResult(HttpMessageHandlerMock.CreateEmptyResponse());
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new TenantApiClient(httpClient);

        // Act
        var act = async () => await client.ActivateAsync(tenantId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ActivateAsync_WhenServerError_ShouldThrowHttpRequestException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var handler = new HttpMessageHandlerMock((request, ct) =>
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new TenantApiClient(httpClient);

        // Act
        var act = async () => await client.ActivateAsync(tenantId);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task DeactivateAsync_WhenSuccessful_ShouldCompleteWithoutException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var handler = new HttpMessageHandlerMock((request, ct) =>
        {
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.ToString().Should().EndWith($"/api/tenants/{tenantId}/deactivate");

            return Task.FromResult(HttpMessageHandlerMock.CreateEmptyResponse());
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new TenantApiClient(httpClient);

        // Act
        var act = async () => await client.DeactivateAsync(tenantId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeactivateAsync_WhenServerError_ShouldThrowHttpRequestException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        var handler = new HttpMessageHandlerMock((request, ct) =>
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new TenantApiClient(httpClient);

        // Act
        var act = async () => await client.DeactivateAsync(tenantId);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        CancellationToken? receivedToken = null;

        var handler = new HttpMessageHandlerMock((request, ct) =>
        {
            receivedToken = ct;
            return Task.FromResult(HttpMessageHandlerMock.CreateJsonResponse(new List<TenantViewModel>()));
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var client = new TenantApiClient(httpClient);

        // Act
        await client.GetAllAsync(cts.Token);

        // Assert
        receivedToken.Should().NotBeNull();
        receivedToken!.Value.CanBeCanceled.Should().BeTrue("CancellationToken should be cancellable");
    }
}
