using System.Net;
using System.Net.Http.Json;
using Cheetah.Identity.Client;
using Cheetah.Identity.Shared.Requests;
using Cheetah.Identity.Shared.ViewModels;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace Cheetah.Identity.Client.Tests;

public class IdentityClientTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly IdentityClient _client;

    public IdentityClientTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://localhost:5001")
        };

        _httpClientFactoryMock
            .Setup(x => x.CreateClient("CheetahAPI"))
            .Returns(httpClient);

        _client = new IdentityClient(_httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WhenSuccessful_ReturnsUserViewModel()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Email = "test@example.com",
            Password = "Test@123",
            FirstName = "John",
            LastName = "Doe"
        };

        var expectedUser = new UserViewModel
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = false,
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains("/api/identity/register")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = JsonContent.Create(expectedUser)
            });

        // Act
        var result = await _client.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(request.Email);
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
    }

    [Fact]
    public async Task RegisterAsync_WhenUserAlreadyExists_ThrowsException()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Email = "existing@example.com",
            Password = "Test@123",
            FirstName = "John",
            LastName = "Doe"
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Conflict,
                Content = new StringContent("User already exists")
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            async () => await _client.RegisterAsync(request));
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new UserViewModel
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"/api/identity/users/{userId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedUser)
            });

        // Act
        var result = await _client.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Email.Should().Be(expectedUser.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserNotFound_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var result = await _client.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByEmailAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var email = "test@example.com";
        var expectedUser = new UserViewModel
        {
            Id = Guid.NewGuid(),
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"/api/identity/users/email/{email}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedUser)
            });

        // Act
        var result = await _client.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WhenUserNotFound_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var result = await _client.GetUserByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenSuccessful_DoesNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest
        {
            CurrentPassword = "OldPassword@123",
            NewPassword = "NewPassword@123"
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains($"/api/identity/users/{userId}/change-password")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        var act = async () => await _client.ChangePasswordAsync(userId, request);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetUserPermissionsAsync_ReturnsListOfPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedPermissions = new List<string>
        {
            "Users.View",
            "Users.Create",
            "Users.Edit"
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"/api/identity/users/{userId}/permissions")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedPermissions)
            });

        // Act
        var result = await _client.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain("Users.View");
        result.Should().Contain("Users.Create");
        result.Should().Contain("Users.Edit");
    }

    [Fact]
    public async Task CreateRoleAsync_WhenSuccessful_ReturnsRoleId()
    {
        // Arrange
        var request = new CreateRoleRequest
        {
            Name = "Manager",
            Description = "Manager role"
        };
        var expectedRoleId = Guid.NewGuid();
        var roleResponse = new { Id = expectedRoleId };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains("/api/identity/roles")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = JsonContent.Create(roleResponse)
            });

        // Act
        var result = await _client.CreateRoleAsync(request);

        // Assert
        result.Should().Be(expectedRoleId);
    }

    [Fact]
    public async Task GetAllRolesAsync_ReturnsListOfRoles()
    {
        // Arrange
        var expectedRoles = new List<RoleViewModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Admin", Description = "Administrator", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "User", Description = "Standard User", CreatedAt = DateTime.UtcNow }
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains("/api/identity/roles")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedRoles)
            });

        // Act
        var result = await _client.GetAllRolesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Name == "Admin");
        result.Should().Contain(r => r.Name == "User");
    }

    [Fact]
    public async Task AssignRoleToUserAsync_WhenSuccessful_DoesNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains($"/api/identity/users/{userId}/roles/{roleId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        var act = async () => await _client.AssignRoleToUserAsync(userId, roleId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AddPermissionToRoleAsync_WhenSuccessful_DoesNotThrow()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permission = "Users.Delete";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains($"/api/identity/roles/{roleId}/permissions")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        var act = async () => await _client.AddPermissionToRoleAsync(roleId, permission);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AddPermissionToUserAsync_WhenSuccessful_DoesNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var permission = "System.Configure";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains($"/api/identity/users/{userId}/permissions")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        var act = async () => await _client.AddPermissionToUserAsync(userId, permission);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ConfirmEmailAsync_WhenSuccessful_DoesNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains($"/api/identity/confirm-email/{userId}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        var act = async () => await _client.ConfirmEmailAsync(userId);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
