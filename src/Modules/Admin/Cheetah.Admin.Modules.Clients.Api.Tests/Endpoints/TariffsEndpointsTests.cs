using System.Net;
using System.Net.Http.Json;
using Cheetah.Admin.Modules.Clients.Api.Tests.Fixtures;
using Cheetah.Admin.Modules.Clients.Contracts.Requests;
using Cheetah.Admin.Modules.Clients.Contracts.Response;
using Shouldly;

namespace Cheetah.Admin.Modules.Clients.Api.Tests.Endpoints;

[Collection("ClientsApi")]
public class TariffsEndpointsTests
{
    private readonly HttpClient _client;

    public TariffsEndpointsTests(ClientsApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    #region GET /api/tariffs

    [Fact]
    public async Task GetAllTariffs_WhenNoTariffs_ShouldReturnEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/tariffs");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tariffs = await response.Content.ReadFromJsonAsync<List<TariffViewModel>>();
        tariffs.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetAllTariffs_AfterCreatingTariffs_ShouldReturnTariffs()
    {
        // Arrange
        var createRequest = new CreateTariffRequest("Test Tariff for List", "Description", 19.99m, "USD", true);
        await _client.PostAsJsonAsync("/api/tariffs", createRequest);

        // Act
        var response = await _client.GetAsync("/api/tariffs");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tariffs = await response.Content.ReadFromJsonAsync<List<TariffViewModel>>();
        tariffs.ShouldNotBeNull();
        tariffs.ShouldContain(t => t.Name == "Test Tariff for List");
    }

    #endregion

    #region POST /api/tariffs

    [Fact]
    public async Task CreateTariff_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateTariffRequest("New Tariff", "New Description", 29.99m, "EUR", true);

        // Act
        var response = await _client.PostAsJsonAsync("/api/tariffs", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task CreateTariff_WithNullDescription_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateTariffRequest("Tariff Without Description", null, 9.99m, "USD", true);

        // Act
        var response = await _client.PostAsJsonAsync("/api/tariffs", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateTariff_WithZeroPrice_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateTariffRequest("Free Tariff", "Free tier", 0m, "USD", true);

        // Act
        var response = await _client.PostAsJsonAsync("/api/tariffs", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateTariff_WithInactiveTariff_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateTariffRequest("Inactive Tariff", "Not active", 49.99m, "GBP", false);

        // Act
        var response = await _client.PostAsJsonAsync("/api/tariffs", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        // Verify the tariff is inactive
        var location = response.Headers.Location;
        var getResponse = await _client.GetAsync(location);
        var tariff = await getResponse.Content.ReadFromJsonAsync<TariffViewModel>();
        tariff!.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task CreateTariff_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateTariffRequest("", "Description", 10m, "USD", true);

        // Act
        var response = await _client.PostAsJsonAsync("/api/tariffs", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTariff_WithInvalidCurrency_ShouldReturnBadRequest()
    {
        // Arrange - currency must be exactly 3 characters
        var request = new CreateTariffRequest("Invalid Currency Tariff", null, 10m, "US", true);

        // Act
        var response = await _client.PostAsJsonAsync("/api/tariffs", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTariff_WithNegativePrice_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateTariffRequest("Negative Price Tariff", null, -10m, "USD", true);

        // Act
        var response = await _client.PostAsJsonAsync("/api/tariffs", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET /api/tariffs/{id}

    [Fact]
    public async Task GetTariffById_WithExistingTariff_ShouldReturnTariff()
    {
        // Arrange
        var createRequest = new CreateTariffRequest("Tariff To Get", "Description", 15.99m, "USD", true);
        var createResponse = await _client.PostAsJsonAsync("/api/tariffs", createRequest);
        var location = createResponse.Headers.Location;

        // Act
        var response = await _client.GetAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var tariff = await response.Content.ReadFromJsonAsync<TariffViewModel>();
        tariff.ShouldNotBeNull();
        tariff!.Name.ShouldBe("Tariff To Get");
        tariff.Price.ShouldBe(15.99m);
        tariff.Currency.ShouldBe("USD");
        tariff.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task GetTariffById_WithNonExistingTariff_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/tariffs/{nonExistingId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTariffById_WithInvalidGuid_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/tariffs/invalid-guid");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    #endregion

    #region PUT /api/tariffs/{id}

    [Fact]
    public async Task UpdateTariff_WithExistingTariff_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreateTariffRequest("Tariff To Update", "Original", 10m, "USD", true);
        var createResponse = await _client.PostAsJsonAsync("/api/tariffs", createRequest);
        var location = createResponse.Headers.Location;
        var tariffId = Guid.Parse(location!.Segments.Last());

        var updateRequest = new UpdateTariffRequest(tariffId, "Updated Tariff", "Updated Description", 25.99m, "EUR", false);

        // Act
        var response = await _client.PutAsJsonAsync(location.ToString(), updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await _client.GetAsync(location);
        var updatedTariff = await getResponse.Content.ReadFromJsonAsync<TariffViewModel>();
        updatedTariff!.Name.ShouldBe("Updated Tariff");
        updatedTariff.Description.ShouldBe("Updated Description");
        updatedTariff.Price.ShouldBe(25.99m);
        updatedTariff.Currency.ShouldBe("EUR");
        updatedTariff.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task UpdateTariff_WithNonExistingTariff_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var updateRequest = new UpdateTariffRequest(nonExistingId, "Updated Name", null, 20m, "USD", true);

        // Act
        var response = await _client.PutAsJsonAsync($"/api/tariffs/{nonExistingId}", updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTariff_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var createRequest = new CreateTariffRequest("Tariff For Empty Name Test", "Description", 10m, "USD", true);
        var createResponse = await _client.PostAsJsonAsync("/api/tariffs", createRequest);
        var location = createResponse.Headers.Location;
        var tariffId = Guid.Parse(location!.Segments.Last());

        var updateRequest = new UpdateTariffRequest(tariffId, "", "Updated Description", 20m, "USD", true);

        // Act
        var response = await _client.PutAsJsonAsync(location.ToString(), updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTariff_WithInvalidCurrency_ShouldReturnBadRequest()
    {
        // Arrange
        var createRequest = new CreateTariffRequest("Tariff For Currency Test", "Description", 10m, "USD", true);
        var createResponse = await _client.PostAsJsonAsync("/api/tariffs", createRequest);
        var location = createResponse.Headers.Location;
        var tariffId = Guid.Parse(location!.Segments.Last());

        var updateRequest = new UpdateTariffRequest(tariffId, "Updated", null, 20m, "EURO", true);

        // Act
        var response = await _client.PutAsJsonAsync(location.ToString(), updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTariff_ActivateInactiveTariff_ShouldSucceed()
    {
        // Arrange
        var createRequest = new CreateTariffRequest("Initially Inactive", "Description", 10m, "USD", false);
        var createResponse = await _client.PostAsJsonAsync("/api/tariffs", createRequest);
        var location = createResponse.Headers.Location;
        var tariffId = Guid.Parse(location!.Segments.Last());

        var updateRequest = new UpdateTariffRequest(tariffId, "Now Active", "Description", 10m, "USD", true);

        // Act
        var response = await _client.PutAsJsonAsync(location.ToString(), updateRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        var tariff = await getResponse.Content.ReadFromJsonAsync<TariffViewModel>();
        tariff!.IsActive.ShouldBeTrue();
    }

    #endregion

    #region DELETE /api/tariffs/{id}

    [Fact]
    public async Task DeleteTariff_WithExistingTariff_ShouldReturnNoContent()
    {
        // Arrange
        var createRequest = new CreateTariffRequest("Tariff To Delete", "Description", 10m, "USD", true);
        var createResponse = await _client.PostAsJsonAsync("/api/tariffs", createRequest);
        var location = createResponse.Headers.Location;

        // Act
        var response = await _client.DeleteAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync(location);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTariff_WithNonExistingTariff_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/tariffs/{nonExistingId}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTariff_ThenGetAll_ShouldNotContainDeletedTariff()
    {
        // Arrange
        var createRequest = new CreateTariffRequest("Tariff To Delete From List", "Description", 10m, "USD", true);
        var createResponse = await _client.PostAsJsonAsync("/api/tariffs", createRequest);
        var location = createResponse.Headers.Location;
        var tariffId = Guid.Parse(location!.Segments.Last());

        // Delete
        await _client.DeleteAsync(location);

        // Act
        var response = await _client.GetAsync("/api/tariffs");
        var tariffs = await response.Content.ReadFromJsonAsync<List<TariffViewModel>>();

        // Assert
        tariffs.ShouldNotContain(t => t.Id == tariffId);
    }

    #endregion
}
