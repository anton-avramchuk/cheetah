using System.Net;
using System.Net.Http.Json;
using Crm.Identity.Api.Tests.Fixtures;
using Crm.Identity.Contracts.Requests;
using Crm.Identity.Contracts.Response;
using Shouldly;

namespace Crm.Identity.Api.Tests.Endpoints;

[Collection("IdentityApi")]
public class RoleEndpointsTests
{
    private const string BasePath = "/api/roles";
    private readonly HttpClient _client;

    public RoleEndpointsTests(IdentityApiFixture fixture)
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
        var result = await response.Content.ReadFromJsonAsync<List<RoleViewModel>>();
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_WithValidName_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateRoleRequest("test-role-create");

        // Act
        var response = await _client.PostAsJsonAsync(BasePath, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateRoleRequest("");

        // Act
        var response = await _client.PostAsJsonAsync(BasePath, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_WithExistingRole_ShouldReturnRole()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest("test-role-getbyid"));
        var location = createResponse.Headers.Location!;

        // Act
        var response = await _client.GetAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var role = await response.Content.ReadFromJsonAsync<RoleViewModel>();
        role.ShouldNotBeNull();
        role!.Name.ShouldBe("test-role-getbyid");
    }

    [Fact]
    public async Task GetById_WithNonExistingRole_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync($"{BasePath}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithExistingRole_ShouldReturnNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest("test-role-update-old"));
        var location = createResponse.Headers.Location!;
        var entityId = Guid.Parse(location.Segments.Last());

        var updateRequest = new UpdateRoleRequest(entityId, "test-role-update-new");

        // Act
        var response = await _client.PutAsJsonAsync(location.ToString(), updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        var updatedRole = await getResponse.Content.ReadFromJsonAsync<RoleViewModel>();
        updatedRole!.Name.ShouldBe("test-role-update-new");
    }

    [Fact]
    public async Task Delete_WithExistingRole_ShouldReturnNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest("test-role-delete"));
        var location = createResponse.Headers.Location!;

        // Act
        var response = await _client.DeleteAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithNonExistingRole_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.DeleteAsync($"{BasePath}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
