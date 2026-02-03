using System.Net;
using System.Net.Http.Json;
using Cheetah.Admin.Modules.Clients.Api.Tests.Fixtures;
using Cheetah.Admin.Modules.Clients.Contracts.Requests;
using Cheetah.Admin.Modules.Clients.Contracts.Response;
using Shouldly;

namespace Cheetah.Admin.Modules.Clients.Api.Tests.Endpoints;

[Collection("ClientsApi")]
public class ClientsEndpointsTests
{
    private readonly HttpClient _client;

    public ClientsEndpointsTests(ClientsApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    #region GET /api/clients

    [Fact]
    public async Task GetAllClients_WhenNoClients_ShouldReturnEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/clients");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var clients = await response.Content.ReadFromJsonAsync<List<ClientViewModel>>();
        clients.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetAllClients_AfterCreatingClients_ShouldReturnClients()
    {
        // Arrange
        var createRequest = new CreateClientRequest("Test Client for List", "Description");
        await _client.PostAsJsonAsync("/api/clients", createRequest);

        // Act
        var response = await _client.GetAsync("/api/clients");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var clients = await response.Content.ReadFromJsonAsync<List<ClientViewModel>>();
        clients.ShouldNotBeNull();
        clients.ShouldContain(c => c.Name == "Test Client for List");
    }

    #endregion

    #region POST /api/clients

    [Fact]
    public async Task CreateClient_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateClientRequest("New Client", "New Description");

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateClient_WithNullDescription_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateClientRequest("Client Without Description", null);

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateClient_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateClientRequest("", "Description");

        // Act
        var response = await _client.PostAsJsonAsync("/api/clients", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET /api/clients/{id}

    [Fact]
    public async Task GetClientById_WithExistingClient_ShouldReturnClient()
    {
        // Arrange
        var createRequest = new CreateClientRequest("Client To Get", "Description");
        var createResponse = await _client.PostAsJsonAsync("/api/clients", createRequest);
        var location = createResponse.Headers.Location;

        // Act
        var response = await _client.GetAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var client = await response.Content.ReadFromJsonAsync<ClientViewModel>();
        client.ShouldNotBeNull();
        client!.Name.ShouldBe("Client To Get");
    }

    [Fact]
    public async Task GetClientById_WithNonExistingClient_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/clients/{nonExistingId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetClientById_WithInvalidGuid_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/clients/invalid-guid");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    #endregion

    #region PUT /api/clients/{id}

    [Fact]
    public async Task UpdateClient_WithExistingClient_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreateClientRequest("Client To Update", "Original Description");
        var createResponse = await _client.PostAsJsonAsync("/api/clients", createRequest);
        var location = createResponse.Headers.Location;
        var clientId = Guid.Parse(location!.Segments.Last());

        var updateRequest = new UpdateClientRequest(clientId, "Updated Client Name", "Updated Description");

        // Act
        var response = await _client.PutAsJsonAsync(location.ToString(), updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await _client.GetAsync(location);
        var updatedClient = await getResponse.Content.ReadFromJsonAsync<ClientViewModel>();
        updatedClient!.Name.ShouldBe("Updated Client Name");
    }

    [Fact]
    public async Task UpdateClient_WithNonExistingClient_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var updateRequest = new UpdateClientRequest(nonExistingId, "Updated Name", "Updated Description");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/clients/{nonExistingId}", updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateClient_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var createRequest = new CreateClientRequest("Client To Update", "Description");
        var createResponse = await _client.PostAsJsonAsync("/api/clients", createRequest);
        var location = createResponse.Headers.Location;
        var clientId = Guid.Parse(location!.Segments.Last());

        var updateRequest = new UpdateClientRequest(clientId, "", "Updated Description");

        // Act
        var response = await _client.PutAsJsonAsync(location.ToString(), updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    #endregion
}
