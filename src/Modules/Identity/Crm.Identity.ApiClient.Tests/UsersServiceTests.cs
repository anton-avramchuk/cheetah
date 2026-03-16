using System.Net;
using System.Text.Json;
using Cheetah.Contracts.Responses;
using Cheetah.Modules.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;
using Shouldly;

namespace Crm.Identity.ApiClient.Tests;

public class UsersServiceTests
{
    private const string BasePath = "users";

    private static UsersService CreateService(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        return new UsersService(httpClient);
    }

    #region GetAllAsync

    [Fact]
    public async Task GetAllAsync_ShouldReturnUsers()
    {
        // Arrange
        var users = new List<UserGridViewModel>
        {
            new(Guid.NewGuid(), "johndoe", "john@example.com"),
            new(Guid.NewGuid(), "janedoe", "jane@example.com")
        };

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(new GridResult<UserGridViewModel>(users, users.Count)));
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
        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(new GridResult<UserGridViewModel>()));
        var service = CreateService(handler);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Data.ShouldBeEmpty();
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new UserDetailViewModel(userId, "johndoe", "john@example.com", []);

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, JsonSerializer.Serialize(expectedUser));
        var service = CreateService(handler);

        // Act
        var result = await service.GetByIdAsync(userId);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(userId);
        handler.RequestUri!.PathAndQuery.ShouldBe($"/{BasePath}/{userId}");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingUser_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "");
        var service = CreateService(handler);

        // Act
        var result = await service.GetByIdAsync(userId);

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
        var request = new CreateUserRequest("johndoe", "john@example.com", "P@ssw0rd!");
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
        var request = new CreateUserRequest("johndoe", "john@example.com", "P@ssw0rd!");
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
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest(userId, "newname", "new@example.com");
        var handler = new MockHttpMessageHandler(HttpStatusCode.NoContent, "");
        var service = CreateService(handler);

        // Act
        await service.UpdateAsync(userId, request);

        // Assert
        handler.RequestUri!.PathAndQuery.ShouldBe($"/{BasePath}/{userId}");
        handler.Method.ShouldBe(HttpMethod.Put);
    }

    [Fact]
    public async Task UpdateAsync_WithNotFound_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest(userId, "newname", "new@example.com");
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "");
        var service = CreateService(handler);

        // Act
        var act = () => service.UpdateAsync(userId, request).AsTask();

        // Assert
        await Should.ThrowAsync<HttpRequestException>(act);
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_WithExistingUser_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var handler = new MockHttpMessageHandler(HttpStatusCode.NoContent, "");
        var service = CreateService(handler);

        // Act
        await service.DeleteAsync(userId);

        // Assert
        handler.RequestUri!.PathAndQuery.ShouldBe($"/{BasePath}/{userId}");
        handler.Method.ShouldBe(HttpMethod.Delete);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingUser_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, "");
        var service = CreateService(handler);

        // Act
        var act = () => service.DeleteAsync(userId).AsTask();

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
