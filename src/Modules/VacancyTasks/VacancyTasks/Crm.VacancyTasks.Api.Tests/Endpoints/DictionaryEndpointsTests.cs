using System.Net;
using System.Net.Http.Json;
using Cheetah.Contracts.Responses;
using Crm.VacancyTasks.Api.Tests.Fixtures;
using Crm.VacancyTasks.Contracts.Requests;
using Crm.VacancyTasks.Contracts.Response;
using Shouldly;

namespace Crm.VacancyTasks.Api.Tests.Endpoints;

[Collection("VacancyTasksApi")]
public class DictionaryEndpointsTests
{
    private readonly HttpClient _client;

    public DictionaryEndpointsTests(VacancyTasksApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    // ──────────────────────────────────────────────────────────────
    // TaskPriorities
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task TaskPriorities_GetAll_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/task-priorities");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<TaskPriorityViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldNotBeNull();
    }

    [Fact]
    public async Task TaskPriorities_Create_WithValidData_ShouldReturnCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/task-priorities",
            new CreateTaskPriorityRequest("Blocker", 0, "#ff0000"));
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task TaskPriorities_Create_WithEmptyName_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/task-priorities",
            new CreateTaskPriorityRequest("", 1, null));
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TaskPriorities_GetById_Existing_ShouldReturnEntity()
    {
        var created = await _client.PostAsJsonAsync("/api/task-priorities",
            new CreateTaskPriorityRequest("Urgent", 5, "#ffff00"));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.GetAsync($"/api/task-priorities/{id}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var entity = await response.Content.ReadFromJsonAsync<TaskPriorityViewModel>();
        entity!.Name.ShouldBe("Urgent");
    }

    [Fact]
    public async Task TaskPriorities_GetById_NotExisting_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/task-priorities/{Guid.NewGuid()}");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TaskPriorities_Update_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/task-priorities",
            new CreateTaskPriorityRequest("OldPriority", 1, null));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.PutAsJsonAsync($"/api/task-priorities/{id}",
            new UpdateTaskPriorityRequest(id, "NewPriority", 2, "#00ff00"));
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/api/task-priorities/{id}");
        var entity = await get.Content.ReadFromJsonAsync<TaskPriorityViewModel>();
        entity!.Name.ShouldBe("NewPriority");
    }

    [Fact]
    public async Task TaskPriorities_Delete_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/task-priorities",
            new CreateTaskPriorityRequest("ToDelete", 99, null));
        var location = created.Headers.Location!;

        var response = await _client.DeleteAsync(location);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync(location);
        get.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // ──────────────────────────────────────────────────────────────
    // TaskStates
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task TaskStates_GetAll_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/task-states");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<TaskStateViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldNotBeNull();
    }

    [Fact]
    public async Task TaskStates_Create_WithValidData_ShouldReturnCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/task-states",
            new CreateTaskStateRequest("Blocked", 5, "#0000ff", false));
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task TaskStates_Create_WithEmptyName_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/task-states",
            new CreateTaskStateRequest("", 1, null, false));
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task TaskStates_GetById_Existing_ShouldReturnEntity()
    {
        var created = await _client.PostAsJsonAsync("/api/task-states",
            new CreateTaskStateRequest("On Hold", 6, "#00ff00", false));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.GetAsync($"/api/task-states/{id}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var entity = await response.Content.ReadFromJsonAsync<TaskStateViewModel>();
        entity!.Name.ShouldBe("On Hold");
    }

    [Fact]
    public async Task TaskStates_GetById_NotExisting_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/task-states/{Guid.NewGuid()}");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TaskStates_Update_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/task-states",
            new CreateTaskStateRequest("OldState", 1, null, false));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.PutAsJsonAsync($"/api/task-states/{id}",
            new UpdateTaskStateRequest(id, "NewState", 2, "#ff0000", false));
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/api/task-states/{id}");
        var entity = await get.Content.ReadFromJsonAsync<TaskStateViewModel>();
        entity!.Name.ShouldBe("NewState");
    }

    [Fact]
    public async Task TaskStates_Delete_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/task-states",
            new CreateTaskStateRequest("ToDelete", 99, null, false));
        var location = created.Headers.Location!;

        var response = await _client.DeleteAsync(location);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync(location);
        get.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
