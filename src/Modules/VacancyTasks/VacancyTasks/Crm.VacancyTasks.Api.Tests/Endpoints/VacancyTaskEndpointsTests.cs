using System.Net;
using System.Net.Http.Json;
using Cheetah.Contracts.Responses;
using Crm.VacancyTasks.Api.Tests.Fixtures;
using Crm.VacancyTasks.Contracts.Requests;
using Crm.VacancyTasks.Contracts.Response;
using Shouldly;

namespace Crm.VacancyTasks.Api.Tests.Endpoints;

[Collection("VacancyTasksApi")]
public class VacancyTaskEndpointsTests
{
    private const string BasePath = "/api/vacancy-tasks";
    private readonly HttpClient _client;
    private readonly Guid _defaultStateId;

    public VacancyTaskEndpointsTests(VacancyTasksApiFixture fixture)
    {
        _client = fixture.CreateClient();
        _defaultStateId = fixture.DefaultStateId;
    }

    [Fact]
    public async Task GetAll_WhenNoEntities_ShouldReturnEmptyList()
    {
        // Act
        var response = await _client.GetAsync(BasePath);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<VacancyTaskViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateVacancyTaskRequest("New Entity", Guid.NewGuid(), _defaultStateId, "Description", null, null, null);

        // Act
        var response = await _client.PostAsJsonAsync(BasePath, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_WithEmptyTitle_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateVacancyTaskRequest("", Guid.NewGuid(), _defaultStateId, "Description", null, null, null);

        // Act
        var response = await _client.PostAsJsonAsync(BasePath, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_WithExistingEntity_ShouldReturnEntity()
    {
        // Arrange
        var createRequest = new CreateVacancyTaskRequest("Entity To Get", Guid.NewGuid(), _defaultStateId, "Description", null, null, null);
        var createResponse = await _client.PostAsJsonAsync(BasePath, createRequest);
        var location = createResponse.Headers.Location;

        // Act
        var response = await _client.GetAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var entity = await response.Content.ReadFromJsonAsync<VacancyTaskViewModel>();
        entity.ShouldNotBeNull();
        entity!.Title.ShouldBe("Entity To Get");
    }

    [Fact]
    public async Task GetById_WithNonExistingEntity_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync($"{BasePath}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithExistingEntity_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreateVacancyTaskRequest("Entity To Update", Guid.NewGuid(), _defaultStateId, "Original", null, null, null);
        var createResponse = await _client.PostAsJsonAsync(BasePath, createRequest);
        var location = createResponse.Headers.Location;
        var entityId = Guid.Parse(location!.Segments.Last());

        var updateRequest = new UpdateVacancyTaskRequest(entityId, "Updated Entity", "Updated Description", _defaultStateId, null, null);

        // Act
        var response = await _client.PutAsJsonAsync(location.ToString(), updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        var updatedEntity = await getResponse.Content.ReadFromJsonAsync<VacancyTaskViewModel>();
        updatedEntity!.Title.ShouldBe("Updated Entity");
    }

    [Fact]
    public async Task Delete_WithExistingEntity_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreateVacancyTaskRequest("Entity To Delete", Guid.NewGuid(), _defaultStateId, "Description", null, null, null);
        var createResponse = await _client.PostAsJsonAsync(BasePath, createRequest);
        var location = createResponse.Headers.Location;

        // Act
        var response = await _client.DeleteAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
