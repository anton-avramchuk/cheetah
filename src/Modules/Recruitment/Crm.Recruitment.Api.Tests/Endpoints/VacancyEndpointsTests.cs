using System.Net;
using System.Net.Http.Json;
using Crm.Recruitment.Api.Tests.Fixtures;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;
using FluentAssertions;

namespace Crm.Recruitment.Api.Tests.Endpoints;

[Collection("RecruitmentApi")]
public class VacancyEndpointsTests
{
    private readonly HttpClient _client;

    public VacancyEndpointsTests(RecruitmentApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetAll_WhenNoEntities_ShouldReturnEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/recruitment");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var entities = await response.Content.ReadFromJsonAsync<List<VacancyViewModel>>();
        entities.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateVacancyRequest("New Entity", "Description");

        // Act
        var response = await _client.PostAsJsonAsync("/api/recruitment", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateVacancyRequest("", "Description");

        // Act
        var response = await _client.PostAsJsonAsync("/api/recruitment", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_WithExistingEntity_ShouldReturnEntity()
    {
        // Arrange
        var createRequest = new CreateVacancyRequest("Entity To Get", "Description");
        var createResponse = await _client.PostAsJsonAsync("/api/recruitment", createRequest);
        var location = createResponse.Headers.Location;

        // Act
        var response = await _client.GetAsync(location);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var entity = await response.Content.ReadFromJsonAsync<VacancyViewModel>();
        entity.Should().NotBeNull();
        entity!.Name.Should().Be("Entity To Get");
    }

    [Fact]
    public async Task GetById_WithNonExistingEntity_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/recruitment/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithExistingEntity_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreateVacancyRequest("Entity To Update", "Original");
        var createResponse = await _client.PostAsJsonAsync("/api/recruitment", createRequest);
        var location = createResponse.Headers.Location;
        var entityId = Guid.Parse(location!.Segments.Last());

        var updateRequest = new UpdateVacancyRequest(entityId, "Updated Entity", "Updated Description");

        // Act
        var response = await _client.PutAsJsonAsync(location.ToString(), updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        var updatedEntity = await getResponse.Content.ReadFromJsonAsync<VacancyViewModel>();
        updatedEntity!.Name.Should().Be("Updated Entity");
    }

    [Fact]
    public async Task Delete_WithExistingEntity_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreateVacancyRequest("Entity To Delete", "Description");
        var createResponse = await _client.PostAsJsonAsync("/api/recruitment", createRequest);
        var location = createResponse.Headers.Location;

        // Act
        var response = await _client.DeleteAsync(location);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}