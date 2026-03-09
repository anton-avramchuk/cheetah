using System.Linq;
using System.Net;
using System.Net.Http.Json;
using Cheetah.Contracts.Responses;
using Crm.Identity.Api.Tests.Fixtures;
using Crm.Identity.Contracts.Requests;
using Crm.Identity.Contracts.Response;
using Shouldly;

namespace Crm.Identity.Api.Tests.Endpoints;

[Collection("IdentityApi")]
public class RoleEndpointsTests
{
    private const string BasePath = "/api/roles";
    private readonly HttpClient _client;

    public RoleEndpointsTests(IdentityApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnList()
    {
        // Act
        var response = await _client.GetAsync(BasePath);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<RoleViewModel>>();
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetAll_WithPagination_ShouldReturnPagedResult()
    {
        // Arrange
        await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest("paging-role-1"));
        await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest("paging-role-2"));
        await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest("paging-role-3"));

        // Act
        var response = await _client.GetAsync($"{BasePath}?page=1&pageSize=2&filter.field=name&filter.operator=startswith&filter.value=paging-role");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<RoleViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.Count().ShouldBe(2);
        result.Total.ShouldBe(3);
    }

    [Fact]
    public async Task GetAll_WithFilterContains_ShouldReturnMatchingRoles()
    {
        // Arrange
        var uniquePart = Guid.NewGuid().ToString("N")[..8];
        await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest($"filter-match-{uniquePart}"));
        await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest($"filter-other-{uniquePart}"));

        // Act
        var response = await _client.GetAsync(
            $"{BasePath}?page=1&pageSize=20&filter.field=name&filter.operator=contains&filter.value=filter-match-{uniquePart}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<RoleViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldAllBe(r => r.Name.Contains($"filter-match-{uniquePart}"));
        result.Data.Count().ShouldBe(1);
    }

    [Fact]
    public async Task GetAll_WithFilterStartsWith_ShouldReturnMatchingRoles()
    {
        // Arrange
        var uniquePart = Guid.NewGuid().ToString("N")[..8];
        await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest($"starts-{uniquePart}-a"));
        await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest($"starts-{uniquePart}-b"));
        await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest($"other-{uniquePart}"));

        // Act
        var response = await _client.GetAsync(
            $"{BasePath}?page=1&pageSize=20&filter.field=name&filter.operator=startswith&filter.value=starts-{uniquePart}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<RoleViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.Count().ShouldBe(2);
        result.Data.ShouldAllBe(r => r.Name.StartsWith($"starts-{uniquePart}"));
    }

    [Fact]
    public async Task GetAll_WithSortAsc_ShouldReturnSortedRoles()
    {
        // Arrange
        var uniquePart = Guid.NewGuid().ToString("N")[..8];
        await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest($"sort-{uniquePart}-charlie"));
        await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest($"sort-{uniquePart}-alpha"));
        await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest($"sort-{uniquePart}-bravo"));

        // Act
        var response = await _client.GetAsync(
            $"{BasePath}?page=1&pageSize=20&filter.field=name&filter.operator=startswith&filter.value=sort-{uniquePart}&sort[0][field]=name&sort[0][dir]=asc");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<RoleViewModel>>();
        result.ShouldNotBeNull();
        var sortedData = result!.Data.ToList();
        sortedData.Count.ShouldBe(3);
        sortedData[0].Name.ShouldBe($"sort-{uniquePart}-alpha");
        sortedData[1].Name.ShouldBe($"sort-{uniquePart}-bravo");
        sortedData[2].Name.ShouldBe($"sort-{uniquePart}-charlie");
    }

    [Fact]
    public async Task GetAll_WithSortDesc_ShouldReturnSortedRoles()
    {
        // Arrange
        var uniquePart = Guid.NewGuid().ToString("N")[..8];
        await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest($"sortd-{uniquePart}-alpha"));
        await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest($"sortd-{uniquePart}-bravo"));

        // Act
        var response = await _client.GetAsync(
            $"{BasePath}?page=1&pageSize=20&filter.field=name&filter.operator=startswith&filter.value=sortd-{uniquePart}&sort[0][field]=name&sort[0][dir]=desc");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<RoleViewModel>>();
        result.ShouldNotBeNull();
        var sortedDescData = result!.Data.ToList();
        sortedDescData.Count.ShouldBe(2);
        sortedDescData[0].Name.ShouldBe($"sortd-{uniquePart}-bravo");
        sortedDescData[1].Name.ShouldBe($"sortd-{uniquePart}-alpha");
    }

    [Fact]
    public async Task GetAll_WithFilterNoMatch_ShouldReturnEmpty()
    {
        // Act
        var response = await _client.GetAsync(
            $"{BasePath}?page=1&pageSize=20&filter.field=name&filter.operator=eq&filter.value={Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<RoleViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldBeEmpty();
        result.Total.ShouldBe(0);
    }

    [Fact]
    public async Task Create_WithValidName_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateRoleRequest("test-role-create");

        // Act
        var response = await _client.PostAsJsonAsync(BasePath, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateRoleRequest("");

        // Act
        var response = await _client.PostAsJsonAsync(BasePath, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_WithExistingRole_ShouldReturnRole()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest("test-role-getbyid"));
        var location = createResponse.Headers.Location!;

        // Act
        var response = await _client.GetAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var role = await response.Content.ReadFromJsonAsync<RoleViewModel>();
        role.ShouldNotBeNull();
        role!.Name.ShouldBe("test-role-getbyid");
    }

    [Fact]
    public async Task GetById_WithNonExistingRole_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync($"{BasePath}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithExistingRole_ShouldReturnNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest("test-role-update-old"));
        var location = createResponse.Headers.Location!;

        // Send only body fields (no Id) to verify route binding works correctly
        var body = new { Name = "test-role-update-new" };

        // Act
        var response = await _client.PutAsJsonAsync(location.ToString(), body);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        var updatedRole = await getResponse.Content.ReadFromJsonAsync<RoleViewModel>();
        updatedRole!.Name.ShouldBe("test-role-update-new");
    }

    [Fact]
    public async Task Update_WithNonExistingRole_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.PutAsJsonAsync(
            $"{BasePath}/{Guid.NewGuid()}",
            new { Name = "does-not-matter" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithExistingRole_ShouldReturnNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(BasePath, new CreateRoleRequest("test-role-delete"));
        var location = createResponse.Headers.Location!;

        // Act
        var response = await _client.DeleteAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithNonExistingRole_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.DeleteAsync($"{BasePath}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
