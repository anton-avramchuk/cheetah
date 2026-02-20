using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Cheetah.Contracts.Responses;
using Crm.Identity.Api.Tests.Fixtures;
using Crm.Identity.Contracts.Requests;
using Crm.Identity.Contracts.Response;
using Shouldly;
using Xunit;

namespace Crm.Identity.Api.Tests.Endpoints;

[Collection("IdentityApi")]
public class UserIdentityEndpointsTests
{
    private readonly HttpClient _client;

    public UserIdentityEndpointsTests(IdentityApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetAll_WhenNoEntities_ShouldReturnEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/identity");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<UserIdentityViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateUserIdentityRequest("New Entity", "Description");

        // Act
        var response = await _client.PostAsJsonAsync("/api/identity", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateUserIdentityRequest("", "Description");

        // Act
        var response = await _client.PostAsJsonAsync("/api/identity", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_WithExistingEntity_ShouldReturnEntity()
    {
        // Arrange
        var createRequest = new CreateUserIdentityRequest("Entity To Get", "Description");
        var createResponse = await _client.PostAsJsonAsync("/api/identity", createRequest);
        var location = createResponse.Headers.Location;

        // Act
        var response = await _client.GetAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var entity = await response.Content.ReadFromJsonAsync<UserIdentityViewModel>();
        entity.ShouldNotBeNull();
        entity!.Name.ShouldBe("Entity To Get");
    }

    [Fact]
    public async Task GetById_WithNonExistingEntity_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/identity/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithExistingEntity_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreateUserIdentityRequest("Entity To Update", "Original");
        var createResponse = await _client.PostAsJsonAsync("/api/identity", createRequest);
        var location = createResponse.Headers.Location;
        var entityId = Guid.Parse(location!.Segments.Last());

        var updateRequest = new UpdateUserIdentityRequest(entityId, "Updated Entity", "Updated Description");

        // Act
        var response = await _client.PutAsJsonAsync(location.ToString(), updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        var updatedEntity = await getResponse.Content.ReadFromJsonAsync<UserIdentityViewModel>();
        updatedEntity!.Name.ShouldBe("Updated Entity");
    }

    [Fact]
    public async Task Delete_WithExistingEntity_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreateUserIdentityRequest("Entity To Delete", "Description");
        var createResponse = await _client.PostAsJsonAsync("/api/identity", createRequest);
        var location = createResponse.Headers.Location;

        // Act
        var response = await _client.DeleteAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}