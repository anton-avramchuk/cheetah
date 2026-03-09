using System.Net;
using System.Net.Http.Json;
using Cheetah.Contracts.Responses;
using Crm.Identity.Api.Tests.Fixtures;
using Crm.Identity.Contracts.Requests;
using Crm.Identity.Contracts.Response;
using Shouldly;

namespace Crm.Identity.Api.Tests.Endpoints;

[Collection("IdentityApi")]
public class UserEndpointsTests
{
    private const string BasePath = "/api/users";
    private readonly HttpClient _client;

    public UserEndpointsTests(IdentityApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnList()
    {
        // Act
        var response = await _client.GetAsync(BasePath);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<UserViewModel>>();
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateUserRequest("testuser-create", "testcreate@example.com", "Test@1234!");

        // Act
        var response = await _client.PostAsJsonAsync(BasePath, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_WithEmptyUserName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateUserRequest("", "test@example.com", "Test@1234!");

        // Act
        var response = await _client.PostAsJsonAsync(BasePath, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(BasePath,
            new CreateUserRequest("testuser-getbyid", "testgetbyid@example.com", "Test@1234!"));
        var location = createResponse.Headers.Location!;

        // Act
        var response = await _client.GetAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserViewModel>();
        user.ShouldNotBeNull();
        user!.UserName.ShouldBe("testuser-getbyid");
    }

    [Fact]
    public async Task GetById_WithNonExistingUser_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync($"{BasePath}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithExistingUser_ShouldReturnNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(BasePath,
            new CreateUserRequest("testuser-update-old", "testupdateold@example.com", "Test@1234!"));
        var location = createResponse.Headers.Location!;

        // Send only body fields (no Id) to verify route binding works correctly
        var body = new { UserName = "testuser-update-new", Email = "testupdatenew@example.com" };

        // Act
        var response = await _client.PutAsJsonAsync(location.ToString(), body);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        var updatedUser = await getResponse.Content.ReadFromJsonAsync<UserViewModel>();
        updatedUser!.UserName.ShouldBe("testuser-update-new");
    }

    [Fact]
    public async Task Delete_WithExistingUser_ShouldReturnNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(BasePath,
            new CreateUserRequest("testuser-delete", "testdelete@example.com", "Test@1234!"));
        var location = createResponse.Headers.Location!;

        // Act
        var response = await _client.DeleteAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithNonExistingUser_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.DeleteAsync($"{BasePath}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
