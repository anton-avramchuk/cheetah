using System.Net;
using System.Net.Http.Json;
using Cheetah.Contracts.Responses;
using Crm.Recruitment.Api.Tests.Fixtures;
using Crm.Recruitment.Contracts.Requests;
using Crm.Recruitment.Contracts.Response;
using Shouldly;

namespace Crm.Recruitment.Api.Tests.Endpoints;

[Collection("RecruitmentApi")]
public class DictionaryEndpointsTests
{
    private readonly HttpClient _client;

    public DictionaryEndpointsTests(RecruitmentApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    // ──────────────────────────────────────────────────────────────
    // CustomerDirections
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CustomerDirections_GetAll_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/customer-directions");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<CustomerDirectionViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldNotBeNull();
    }

    [Fact]
    public async Task CustomerDirections_Create_WithValidData_ShouldReturnCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/customer-directions",
            new CreateCustomerDirectionRequest("IT", null));
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task CustomerDirections_Create_WithEmptyName_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/customer-directions",
            new CreateCustomerDirectionRequest("", null));
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CustomerDirections_GetById_Existing_ShouldReturnEntity()
    {
        var created = await _client.PostAsJsonAsync("/api/customer-directions",
            new CreateCustomerDirectionRequest("Finance", "Finance dept"));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.GetAsync($"/api/customer-directions/{id}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var entity = await response.Content.ReadFromJsonAsync<CustomerDirectionViewModel>();
        entity!.Name.ShouldBe("Finance");
    }

    [Fact]
    public async Task CustomerDirections_GetById_NotExisting_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/customer-directions/{Guid.NewGuid()}");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CustomerDirections_Update_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/customer-directions",
            new CreateCustomerDirectionRequest("OldName", null));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.PutAsJsonAsync($"/api/customer-directions/{id}",
            new UpdateCustomerDirectionRequest(id, "NewName", "Updated"));
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/api/customer-directions/{id}");
        var entity = await get.Content.ReadFromJsonAsync<CustomerDirectionViewModel>();
        entity!.Name.ShouldBe("NewName");
    }

    [Fact]
    public async Task CustomerDirections_Delete_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/customer-directions",
            new CreateCustomerDirectionRequest("ToDelete", null));
        var location = created.Headers.Location!;

        var response = await _client.DeleteAsync(location);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync(location);
        get.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // ──────────────────────────────────────────────────────────────
    // Customers
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Customers_GetAll_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/customers");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<CustomerViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldNotBeNull();
    }

    [Fact]
    public async Task Customers_Create_WithValidData_ShouldReturnCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest("Acme Corp", null, null, null));
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task Customers_Create_WithEmptyName_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest("", null, null, null));
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Customers_GetById_Existing_ShouldReturnEntity()
    {
        var created = await _client.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest("GlobalTech", "GT", null, null));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.GetAsync($"/api/customers/{id}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var entity = await response.Content.ReadFromJsonAsync<CustomerViewModel>();
        entity!.Name.ShouldBe("GlobalTech");
    }

    [Fact]
    public async Task Customers_GetById_NotExisting_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/customers/{Guid.NewGuid()}");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Customers_Update_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest("OldCorp", null, null, null));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.PutAsJsonAsync($"/api/customers/{id}",
            new UpdateCustomerRequest(id, "NewCorp", "NC", null, null));
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/api/customers/{id}");
        var entity = await get.Content.ReadFromJsonAsync<CustomerViewModel>();
        entity!.Name.ShouldBe("NewCorp");
    }

    [Fact]
    public async Task Customers_Delete_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/customers",
            new CreateCustomerRequest("ToDelete", null, null, null));
        var location = created.Headers.Location!;

        var response = await _client.DeleteAsync(location);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync(location);
        get.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // ──────────────────────────────────────────────────────────────
    // Positions
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Positions_GetAll_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/positions");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<PositionViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldNotBeNull();
    }

    [Fact]
    public async Task Positions_Create_WithValidData_ShouldReturnCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/positions",
            new CreatePositionRequest("Developer"));
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task Positions_Create_WithEmptyName_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/positions",
            new CreatePositionRequest(""));
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Positions_GetById_Existing_ShouldReturnEntity()
    {
        var created = await _client.PostAsJsonAsync("/api/positions",
            new CreatePositionRequest("Designer"));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.GetAsync($"/api/positions/{id}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var entity = await response.Content.ReadFromJsonAsync<PositionViewModel>();
        entity!.Name.ShouldBe("Designer");
    }

    [Fact]
    public async Task Positions_GetById_NotExisting_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/positions/{Guid.NewGuid()}");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Positions_Update_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/positions",
            new CreatePositionRequest("OldPosition"));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.PutAsJsonAsync($"/api/positions/{id}",
            new UpdatePositionRequest(id, "NewPosition"));
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/api/positions/{id}");
        var entity = await get.Content.ReadFromJsonAsync<PositionViewModel>();
        entity!.Name.ShouldBe("NewPosition");
    }

    [Fact]
    public async Task Positions_Delete_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/positions",
            new CreatePositionRequest("ToDelete"));
        var location = created.Headers.Location!;

        var response = await _client.DeleteAsync(location);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync(location);
        get.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // ──────────────────────────────────────────────────────────────
    // StackItems
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task StackItems_GetAll_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/stack-items");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<StackItemViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldNotBeNull();
    }

    [Fact]
    public async Task StackItems_Create_WithValidData_ShouldReturnCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/stack-items",
            new CreateStackItemRequest("C#"));
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task StackItems_Create_WithEmptyName_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/stack-items",
            new CreateStackItemRequest(""));
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task StackItems_GetById_Existing_ShouldReturnEntity()
    {
        var created = await _client.PostAsJsonAsync("/api/stack-items",
            new CreateStackItemRequest("TypeScript"));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.GetAsync($"/api/stack-items/{id}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var entity = await response.Content.ReadFromJsonAsync<StackItemViewModel>();
        entity!.Name.ShouldBe("TypeScript");
    }

    [Fact]
    public async Task StackItems_GetById_NotExisting_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/stack-items/{Guid.NewGuid()}");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task StackItems_Update_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/stack-items",
            new CreateStackItemRequest("OldStack"));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.PutAsJsonAsync($"/api/stack-items/{id}",
            new UpdateStackItemRequest(id, "NewStack"));
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/api/stack-items/{id}");
        var entity = await get.Content.ReadFromJsonAsync<StackItemViewModel>();
        entity!.Name.ShouldBe("NewStack");
    }

    [Fact]
    public async Task StackItems_Delete_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/stack-items",
            new CreateStackItemRequest("ToDelete"));
        var location = created.Headers.Location!;

        var response = await _client.DeleteAsync(location);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync(location);
        get.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // ──────────────────────────────────────────────────────────────
    // VacancyRoles (read-only)
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task VacancyRoles_GetAll_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/vacancy-roles");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<VacancyRoleViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldNotBeNull();
    }

    // ──────────────────────────────────────────────────────────────
    // VacancyStates
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task VacancyStates_GetAll_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/vacancy-states");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<VacancyStateViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldNotBeNull();
    }

    [Fact]
    public async Task VacancyStates_Create_WithValidData_ShouldReturnCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/vacancy-states",
            new CreateVacancyStateRequest("Open", 1, "#00ff00", false));
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task VacancyStates_Create_WithEmptyName_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/vacancy-states",
            new CreateVacancyStateRequest("", 1, null, false));
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VacancyStates_GetById_Existing_ShouldReturnEntity()
    {
        var created = await _client.PostAsJsonAsync("/api/vacancy-states",
            new CreateVacancyStateRequest("Closed", 2, null, false));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.GetAsync($"/api/vacancy-states/{id}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var entity = await response.Content.ReadFromJsonAsync<VacancyStateViewModel>();
        entity!.Name.ShouldBe("Closed");
    }

    [Fact]
    public async Task VacancyStates_GetById_NotExisting_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/vacancy-states/{Guid.NewGuid()}");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task VacancyStates_Update_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/vacancy-states",
            new CreateVacancyStateRequest("OldState", 1, null, false));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.PutAsJsonAsync($"/api/vacancy-states/{id}",
            new UpdateVacancyStateRequest(id, "NewState", 2, "#ff0000", false));
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/api/vacancy-states/{id}");
        var entity = await get.Content.ReadFromJsonAsync<VacancyStateViewModel>();
        entity!.Name.ShouldBe("NewState");
    }

    [Fact]
    public async Task VacancyStates_Delete_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/vacancy-states",
            new CreateVacancyStateRequest("ToDelete", 99, null, false));
        var location = created.Headers.Location!;

        var response = await _client.DeleteAsync(location);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync(location);
        get.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // ──────────────────────────────────────────────────────────────
    // WorkFormats
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task WorkFormats_GetAll_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/work-formats");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<WorkFormatViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldNotBeNull();
    }

    [Fact]
    public async Task WorkFormats_Create_WithValidData_ShouldReturnCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/work-formats",
            new CreateWorkFormatRequest("Remote"));
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task WorkFormats_Create_WithEmptyName_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/work-formats",
            new CreateWorkFormatRequest(""));
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WorkFormats_GetById_Existing_ShouldReturnEntity()
    {
        var created = await _client.PostAsJsonAsync("/api/work-formats",
            new CreateWorkFormatRequest("Hybrid"));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.GetAsync($"/api/work-formats/{id}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var entity = await response.Content.ReadFromJsonAsync<WorkFormatViewModel>();
        entity!.Name.ShouldBe("Hybrid");
    }

    [Fact]
    public async Task WorkFormats_GetById_NotExisting_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/work-formats/{Guid.NewGuid()}");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WorkFormats_Update_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/work-formats",
            new CreateWorkFormatRequest("OldFormat"));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.PutAsJsonAsync($"/api/work-formats/{id}",
            new UpdateWorkFormatRequest(id, "NewFormat"));
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/api/work-formats/{id}");
        var entity = await get.Content.ReadFromJsonAsync<WorkFormatViewModel>();
        entity!.Name.ShouldBe("NewFormat");
    }

    [Fact]
    public async Task WorkFormats_Delete_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/work-formats",
            new CreateWorkFormatRequest("ToDelete"));
        var location = created.Headers.Location!;

        var response = await _client.DeleteAsync(location);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync(location);
        get.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
