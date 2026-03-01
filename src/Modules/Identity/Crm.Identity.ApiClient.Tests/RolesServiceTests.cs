using System.Net;
using System.Text.Json;
using Cheetah.Contracts.Responses;
using Crm.Identity.Contracts.Requests;
using Crm.Identity.Contracts.Response;
using Shouldly;

namespace Crm.Identity.ApiClient.Tests;

public class RolesServiceTests
{
    private const string BasePath = "roles";

    private static RolesService CreateService(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        return new RolesService(httpClient);
    }

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ShouldReturnRoles()
    {
        // Arrange
        var roles = new List<RoleViewModel>
        {
            new(Guid.NewGuid(), "admin"),
            new(Guid.NewGuid(), "recruiter")
        };

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(new GridResult<RoleViewModel>(roles, roles.Count)));
        var service = CreateService(handler);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Total.ShouldBe(2);
        handler.RequestUri!.PathAndQuery.ShouldStartWith($"/{BasePath}");
        handler.Method.ShouldBe(HttpMethod.Get);
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(new GridResult<RoleViewModel>()));
        var service = CreateService(handler);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Data.ShouldBeEmpty();
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WithExistingRole_ShouldReturnRole()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var expectedRole = new RoleViewModel(roleId, "admin");

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(expectedRole));
        var service = CreateService(handler);

        // Act
        var result = await service.GetByIdAsync(roleId);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(roleId);
        handler.RequestUri!.PathAndQuery.ShouldBe($"/{BasePath}/{roleId}");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingRole_ShouldReturnNull()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "");
        var service = CreateService(handler);

        // Act
        var result = await service.GetByIdAsync(roleId);

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
        var request = new CreateRoleRequest("admin");
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
        var request = new CreateRoleRequest("admin");
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
        var roleId = Guid.NewGuid();
        var request = new UpdateRoleRequest(roleId, "updated-name");
        var handler = new MockHttpMessageHandler(HttpStatusCode.NoContent, "");
        var service = CreateService(handler);

        // Act
        await service.UpdateAsync(roleId, request);

        // Assert
        handler.RequestUri!.PathAndQuery.ShouldBe($"/{BasePath}/{roleId}");
        handler.Method.ShouldBe(HttpMethod.Put);
    }

    [Fact]
    public async Task UpdateAsync_WithNotFound_ShouldThrow()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var request = new UpdateRoleRequest(roleId, "updated-name");
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "");
        var service = CreateService(handler);

        // Act
        var act = () => service.UpdateAsync(roleId, request).AsTask();

        // Assert
        await Should.ThrowAsync<HttpRequestException>(act);
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_WithExistingRole_ShouldSucceed()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var handler = new MockHttpMessageHandler(HttpStatusCode.NoContent, "");
        var service = CreateService(handler);

        // Act
        await service.DeleteAsync(roleId);

        // Assert
        handler.RequestUri!.PathAndQuery.ShouldBe($"/{BasePath}/{roleId}");
        handler.Method.ShouldBe(HttpMethod.Delete);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingRole_ShouldThrow()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "");
        var service = CreateService(handler);

        // Act
        var act = () => service.DeleteAsync(roleId).AsTask();

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
