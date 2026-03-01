using System.Net;
using System.Net.Http.Json;
using Cheetah.Contracts.Responses;
using Crm.Candidates.Api.Tests.Fixtures;
using Crm.Candidates.Contracts.Requests;
using Crm.Candidates.Contracts.Response;
using Shouldly;

namespace Crm.Candidates.Api.Tests.Endpoints;

[Collection("CandidatesApi")]
public class DictionaryEndpointsTests
{
    private readonly HttpClient _client;

    public DictionaryEndpointsTests(CandidatesApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    // ──────────────────────────────────────────────────────────────
    // CandidateSources
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CandidateSources_GetAll_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/candidate-sources");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<CandidateSourceViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldNotBeNull();
    }

    [Fact]
    public async Task CandidateSources_Create_WithValidData_ShouldReturnCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/candidate-sources",
            new CreateCandidateSourceRequest("LinkedIn", 1, "#0077b5"));
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task CandidateSources_Create_WithEmptyName_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/candidate-sources",
            new CreateCandidateSourceRequest("", 1, null));
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CandidateSources_GetById_Existing_ShouldReturnEntity()
    {
        var created = await _client.PostAsJsonAsync("/api/candidate-sources",
            new CreateCandidateSourceRequest("Referral", 2, null));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.GetAsync($"/api/candidate-sources/{id}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var entity = await response.Content.ReadFromJsonAsync<CandidateSourceViewModel>();
        entity!.Name.ShouldBe("Referral");
    }

    [Fact]
    public async Task CandidateSources_GetById_NotExisting_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/candidate-sources/{Guid.NewGuid()}");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CandidateSources_Update_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/candidate-sources",
            new CreateCandidateSourceRequest("OldSource", 1, null));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.PutAsJsonAsync($"/api/candidate-sources/{id}",
            new UpdateCandidateSourceRequest(id, "NewSource", 2, "#ff0000"));
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/api/candidate-sources/{id}");
        var entity = await get.Content.ReadFromJsonAsync<CandidateSourceViewModel>();
        entity!.Name.ShouldBe("NewSource");
    }

    [Fact]
    public async Task CandidateSources_Delete_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/candidate-sources",
            new CreateCandidateSourceRequest("ToDelete", 99, null));
        var location = created.Headers.Location!;

        var response = await _client.DeleteAsync(location);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync(location);
        get.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    // ──────────────────────────────────────────────────────────────
    // CandidateStages
    // ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CandidateStages_GetAll_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/candidate-stages");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<CandidateStageViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldNotBeNull();
    }

    [Fact]
    public async Task CandidateStages_Create_WithValidData_ShouldReturnCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/candidate-stages",
            new CreateCandidateStageRequest("Screening", 1, "#3498db", false));
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task CandidateStages_Create_WithEmptyName_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/candidate-stages",
            new CreateCandidateStageRequest("", 1, null, false));
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CandidateStages_GetById_Existing_ShouldReturnEntity()
    {
        var created = await _client.PostAsJsonAsync("/api/candidate-stages",
            new CreateCandidateStageRequest("Interview", 2, "#2ecc71", false));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.GetAsync($"/api/candidate-stages/{id}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var entity = await response.Content.ReadFromJsonAsync<CandidateStageViewModel>();
        entity!.Name.ShouldBe("Interview");
    }

    [Fact]
    public async Task CandidateStages_GetById_NotExisting_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/candidate-stages/{Guid.NewGuid()}");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CandidateStages_Update_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/candidate-stages",
            new CreateCandidateStageRequest("OldStage", 1, null, false));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.PutAsJsonAsync($"/api/candidate-stages/{id}",
            new UpdateCandidateStageRequest(id, "NewStage", 2, "#e74c3c", false));
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"/api/candidate-stages/{id}");
        var entity = await get.Content.ReadFromJsonAsync<CandidateStageViewModel>();
        entity!.Name.ShouldBe("NewStage");
    }

    [Fact]
    public async Task CandidateStages_Delete_ShouldReturnNoContent()
    {
        var created = await _client.PostAsJsonAsync("/api/candidate-stages",
            new CreateCandidateStageRequest("ToDelete", 99, null, false));
        var location = created.Headers.Location!;

        var response = await _client.DeleteAsync(location);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync(location);
        get.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
