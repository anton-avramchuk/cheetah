using System.Net;
using System.Net.Http.Json;
using Cheetah.Contracts.Responses;
using Crm.Customer.Api.Tests.Fixtures;
using Crm.Customer.Contracts.Requests;
using Crm.Customer.Contracts.Response;
using Shouldly;

namespace Crm.Customer.Api.Tests.Endpoints;

[Collection("CustomerApi")]
public class CustomerEndpointsTests
{
    private readonly HttpClient _client;

    public CustomerEndpointsTests(CustomerApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetAll_WhenNoEntities_ShouldReturnEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/customer");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<CustomerViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateCustomerRequest("New Entity", "Description");

        // Act
        var response = await _client.PostAsJsonAsync("/api/customer", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateCustomerRequest("", "Description");

        // Act
        var response = await _client.PostAsJsonAsync("/api/customer", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_WithExistingEntity_ShouldReturnEntity()
    {
        // Arrange
        var createRequest = new CreateCustomerRequest("Entity To Get", "Description");
        var createResponse = await _client.PostAsJsonAsync("/api/customer", createRequest);
        var location = createResponse.Headers.Location;

        // Act
        var response = await _client.GetAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var entity = await response.Content.ReadFromJsonAsync<CustomerViewModel>();
        entity.ShouldNotBeNull();
        entity!.Name.ShouldBe("Entity To Get");
    }

    [Fact]
    public async Task GetById_WithNonExistingEntity_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/customer/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithExistingEntity_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreateCustomerRequest("Entity To Update", "Original");
        var createResponse = await _client.PostAsJsonAsync("/api/customer", createRequest);
        var location = createResponse.Headers.Location;
        var entityId = Guid.Parse(location!.Segments.Last());

        var updateRequest = new UpdateCustomerRequest(entityId, "Updated Entity", "Updated Description");

        // Act
        var response = await _client.PutAsJsonAsync(location.ToString(), updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        var updatedEntity = await getResponse.Content.ReadFromJsonAsync<CustomerViewModel>();
        updatedEntity!.Name.ShouldBe("Updated Entity");
    }

    [Fact]
    public async Task Delete_WithExistingEntity_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreateCustomerRequest("Entity To Delete", "Description");
        var createResponse = await _client.PostAsJsonAsync("/api/customer", createRequest);
        var location = createResponse.Headers.Location;

        // Act
        var response = await _client.DeleteAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}