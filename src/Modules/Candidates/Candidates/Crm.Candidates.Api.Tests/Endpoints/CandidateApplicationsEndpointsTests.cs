using System.Net;
using System.Net.Http.Json;
using Cheetah.Contracts.Responses;
using Crm.Candidates.Api.Tests.Fixtures;
using Crm.Candidates.Contracts.Requests;
using Crm.Candidates.Contracts.Response;
using Shouldly;

namespace Crm.Candidates.Api.Tests.Endpoints;

[Collection("CandidatesApi")]
public class CandidateApplicationsEndpointsTests
{
    private const string BasePath = "/api/candidate-applications";
    private readonly HttpClient _client;

    public CandidateApplicationsEndpointsTests(CandidatesApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    private async Task<Guid> CreateCandidateAsync(string firstName, string lastName)
    {
        var response = await _client.PostAsJsonAsync("/api/candidates",
            new CreateCandidateRequest(firstName, lastName, null, null, null, null, null, null, null));
        response.EnsureSuccessStatusCode();
        return Guid.Parse(response.Headers.Location!.Segments.Last());
    }

    private async Task<Guid> CreateStageAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/candidate-stages",
            new CreateCandidateStageRequest(name, 1, null, false));
        response.EnsureSuccessStatusCode();
        return Guid.Parse(response.Headers.Location!.Segments.Last());
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        var response = await _client.GetAsync(BasePath);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<CandidateApplicationViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        var candidateId = await CreateCandidateAsync("App", "Create");
        var stageId = await CreateStageAsync("AppStage-Create");

        var response = await _client.PostAsJsonAsync(BasePath,
            new CreateCandidateApplicationRequest(candidateId, Guid.NewGuid(), stageId, 1));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetById_WithExistingEntity_ShouldReturnEntity()
    {
        var candidateId = await CreateCandidateAsync("App", "GetById");
        var stageId = await CreateStageAsync("AppStage-GetById");
        var vacancyId = Guid.NewGuid();

        var created = await _client.PostAsJsonAsync(BasePath,
            new CreateCandidateApplicationRequest(candidateId, vacancyId, stageId, 2));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.GetAsync($"{BasePath}/{id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var entity = await response.Content.ReadFromJsonAsync<CandidateApplicationViewModel>();
        entity.ShouldNotBeNull();
        entity!.CandidateId.ShouldBe(candidateId);
        entity.StageId.ShouldBe(stageId);
        entity.VacancyId.ShouldBe(vacancyId);
    }

    [Fact]
    public async Task GetById_WithNonExistingEntity_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"{BasePath}/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithExistingEntity_ShouldReturnNoContent()
    {
        var candidateId = await CreateCandidateAsync("App", "Delete");
        var stageId = await CreateStageAsync("AppStage-Delete");

        var created = await _client.PostAsJsonAsync(BasePath,
            new CreateCandidateApplicationRequest(candidateId, Guid.NewGuid(), stageId, 1));
        var location = created.Headers.Location!;

        var response = await _client.DeleteAsync(location);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync(location);
        get.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Move_WithExistingEntity_ShouldUpdateStageAndOrder()
    {
        var candidateId = await CreateCandidateAsync("App", "Move");
        var stageId = await CreateStageAsync("AppStage-MoveFrom");
        var newStageId = await CreateStageAsync("AppStage-MoveTo");

        var created = await _client.PostAsJsonAsync(BasePath,
            new CreateCandidateApplicationRequest(candidateId, Guid.NewGuid(), stageId, 1));
        var id = Guid.Parse(created.Headers.Location!.Segments.Last());

        var response = await _client.PatchAsJsonAsync($"{BasePath}/{id}/move",
            new { StageId = newStageId, Order = 5 });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var get = await _client.GetAsync($"{BasePath}/{id}");
        var entity = await get.Content.ReadFromJsonAsync<CandidateApplicationViewModel>();
        entity!.StageId.ShouldBe(newStageId);
        entity.Order.ShouldBe(5);
    }

    [Fact]
    public async Task Move_WithNonExistingEntity_ShouldReturnNotFound()
    {
        var response = await _client.PatchAsJsonAsync($"{BasePath}/{Guid.NewGuid()}/move",
            new { StageId = Guid.NewGuid(), Order = 1 });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
