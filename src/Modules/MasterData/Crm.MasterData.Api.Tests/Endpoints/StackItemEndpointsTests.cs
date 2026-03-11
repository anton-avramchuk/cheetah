using System.Net;
using System.Net.Http.Json;
using Cheetah.Contracts.Responses;
using Crm.MasterData.Api.Tests.Fixtures;
using Crm.MasterData.Contracts.Requests;
using Crm.MasterData.Contracts.Response;
using Shouldly;

namespace Crm.MasterData.Api.Tests.Endpoints;

[Collection("MasterDataApi")]
public class StackItemEndpointsTests
{
    private readonly HttpClient _client;

    public StackItemEndpointsTests(MasterDataApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetAll_WhenNoEntities_ShouldReturnEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/masterdata");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<StackItemViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateStackItemRequest("New Entity", "Description");

        // Act
        var response = await _client.PostAsJsonAsync("/api/masterdata", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateStackItemRequest("", "Description");

        // Act
        var response = await _client.PostAsJsonAsync("/api/masterdata", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_WithExistingEntity_ShouldReturnEntity()
    {
        // Arrange
        var createRequest = new CreateStackItemRequest("Entity To Get", "Description");
        var createResponse = await _client.PostAsJsonAsync("/api/masterdata", createRequest);
        var location = createResponse.Headers.Location;

        // Act
        var response = await _client.GetAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var entity = await response.Content.ReadFromJsonAsync<StackItemViewModel>();
        entity.ShouldNotBeNull();
        entity!.Name.ShouldBe("Entity To Get");
    }

    [Fact]
    public async Task GetById_WithNonExistingEntity_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/masterdata/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithExistingEntity_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreateStackItemRequest("Entity To Update", "Original");
        var createResponse = await _client.PostAsJsonAsync("/api/masterdata", createRequest);
        var location = createResponse.Headers.Location;
        var entityId = Guid.Parse(location!.Segments.Last());

        var updateRequest = new UpdateStackItemRequest(entityId, "Updated Entity", "Updated Description");

        // Act
        var response = await _client.PutAsJsonAsync(location.ToString(), updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        var updatedEntity = await getResponse.Content.ReadFromJsonAsync<StackItemViewModel>();
        updatedEntity!.Name.ShouldBe("Updated Entity");
    }

    [Fact]
    public async Task Delete_WithExistingEntity_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreateStackItemRequest("Entity To Delete", "Description");
        var createResponse = await _client.PostAsJsonAsync("/api/masterdata", createRequest);
        var location = createResponse.Headers.Location;

        // Act
        var response = await _client.DeleteAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}