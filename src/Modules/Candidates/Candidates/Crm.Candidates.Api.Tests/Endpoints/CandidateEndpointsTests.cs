using System.Net;
using System.Net.Http.Json;
using Cheetah.Contracts.Responses;
using Crm.Candidates.Api.Tests.Fixtures;
using Crm.Candidates.Contracts.Requests;
using Crm.Candidates.Contracts.Response;
using Shouldly;

namespace Crm.Candidates.Api.Tests.Endpoints;

[Collection("CandidatesApi")]
public class CandidateEndpointsTests
{
    private readonly HttpClient _client;

    public CandidateEndpointsTests(CandidatesApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetAll_WhenNoEntities_ShouldReturnEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/candidates");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<CandidateViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateCandidateRequest("John", "Doe", "john@test.com", null, null, null, null, null, null);

        // Act
        var response = await _client.PostAsJsonAsync("/api/candidates", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_WithEmptyFirstName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateCandidateRequest("", "Doe", null, null, null, null, null, null, null);

        // Act
        var response = await _client.PostAsJsonAsync("/api/candidates", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_WithExistingEntity_ShouldReturnEntity()
    {
        // Arrange
        var createRequest = new CreateCandidateRequest("Jane", "Smith", null, null, null, null, null, null, null);
        var createResponse = await _client.PostAsJsonAsync("/api/candidates", createRequest);
        var location = createResponse.Headers.Location;

        // Act
        var response = await _client.GetAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var entity = await response.Content.ReadFromJsonAsync<CandidateViewModel>();
        entity.ShouldNotBeNull();
        entity!.FirstName.ShouldBe("Jane");
        entity.LastName.ShouldBe("Smith");
    }

    [Fact]
    public async Task GetById_WithNonExistingEntity_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/candidates/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithExistingEntity_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreateCandidateRequest("Update", "Test", null, null, null, null, null, null, null);
        var createResponse = await _client.PostAsJsonAsync("/api/candidates", createRequest);
        var location = createResponse.Headers.Location;
        var entityId = Guid.Parse(location!.Segments.Last());

        var updateRequest = new UpdateCandidateRequest(entityId, "Updated", "Name", "updated@test.com", null, null, null, null, null, null);

        // Act
        var response = await _client.PutAsJsonAsync(location.ToString(), updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        var updatedEntity = await getResponse.Content.ReadFromJsonAsync<CandidateViewModel>();
        updatedEntity!.FirstName.ShouldBe("Updated");
        updatedEntity.LastName.ShouldBe("Name");
    }

    [Fact]
    public async Task Delete_WithExistingEntity_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreateCandidateRequest("Delete", "Me", null, null, null, null, null, null, null);
        var createResponse = await _client.PostAsJsonAsync("/api/candidates", createRequest);
        var location = createResponse.Headers.Location;

        // Act
        var response = await _client.DeleteAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
