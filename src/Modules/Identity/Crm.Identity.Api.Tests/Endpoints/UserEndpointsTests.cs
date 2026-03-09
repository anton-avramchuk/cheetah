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
public class UserEndpointsTests
{
    private const string BasePath = "/api/users";
    private readonly HttpClient _client;

    public UserEndpointsTests(IdentityApiFixture fixture)
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
        var result = await response.Content.ReadFromJsonAsync<GridResult<UserGridViewModel>>();
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetAll_WithPagination_ShouldReturnPagedResult()
    {
        // Arrange
        var uniquePart = Guid.NewGuid().ToString("N")[..8];
        await _client.PostAsJsonAsync(BasePath, new CreateUserRequest($"paging-user1-{uniquePart}", $"p1-{uniquePart}@test.com", "Test@1234!"));
        await _client.PostAsJsonAsync(BasePath, new CreateUserRequest($"paging-user2-{uniquePart}", $"p2-{uniquePart}@test.com", "Test@1234!"));
        await _client.PostAsJsonAsync(BasePath, new CreateUserRequest($"paging-user3-{uniquePart}", $"p3-{uniquePart}@test.com", "Test@1234!"));

        // Act
        var response = await _client.GetAsync(
            $"{BasePath}?page=1&pageSize=2&filter.field=userName&filter.operator=startswith&filter.value=paging-user");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<UserGridViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.Count().ShouldBe(2);
        result.Total.ShouldBe(3);
    }

    [Fact]
    public async Task GetAll_WithFilterContains_ShouldReturnMatchingUsers()
    {
        // Arrange
        var uniquePart = Guid.NewGuid().ToString("N")[..8];
        await _client.PostAsJsonAsync(BasePath, new CreateUserRequest($"filter-match-{uniquePart}", $"match-{uniquePart}@test.com", "Test@1234!"));
        await _client.PostAsJsonAsync(BasePath, new CreateUserRequest($"filter-other-{uniquePart}", $"other-{uniquePart}@test.com", "Test@1234!"));

        // Act
        var response = await _client.GetAsync(
            $"{BasePath}?page=1&pageSize=20&filter.field=userName&filter.operator=contains&filter.value=filter-match-{uniquePart}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<UserGridViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.Count().ShouldBe(1);
        result.Data.First().UserName.ShouldBe($"filter-match-{uniquePart}");
    }

    [Fact]
    public async Task GetAll_WithSortAsc_ShouldReturnSortedUsers()
    {
        // Arrange
        var uniquePart = Guid.NewGuid().ToString("N")[..8];
        await _client.PostAsJsonAsync(BasePath, new CreateUserRequest($"sort-{uniquePart}-charlie", $"sc-{uniquePart}@test.com", "Test@1234!"));
        await _client.PostAsJsonAsync(BasePath, new CreateUserRequest($"sort-{uniquePart}-alpha", $"sa-{uniquePart}@test.com", "Test@1234!"));
        await _client.PostAsJsonAsync(BasePath, new CreateUserRequest($"sort-{uniquePart}-bravo", $"sb-{uniquePart}@test.com", "Test@1234!"));

        // Act
        var response = await _client.GetAsync(
            $"{BasePath}?page=1&pageSize=20&filter.field=userName&filter.operator=startswith&filter.value=sort-{uniquePart}&sort[0][field]=userName&sort[0][dir]=asc");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<UserGridViewModel>>();
        result.ShouldNotBeNull();
        var sortedData = result!.Data.ToList();
        sortedData.Count.ShouldBe(3);
        sortedData[0].UserName.ShouldBe($"sort-{uniquePart}-alpha");
        sortedData[1].UserName.ShouldBe($"sort-{uniquePart}-bravo");
        sortedData[2].UserName.ShouldBe($"sort-{uniquePart}-charlie");
    }

    [Fact]
    public async Task GetAll_WithFilterNoMatch_ShouldReturnEmpty()
    {
        // Act
        var response = await _client.GetAsync(
            $"{BasePath}?page=1&pageSize=20&filter.field=userName&filter.operator=eq&filter.value={Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GridResult<UserGridViewModel>>();
        result.ShouldNotBeNull();
        result!.Data.ShouldBeEmpty();
        result.Total.ShouldBe(0);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateUserRequest("testuser-create", "testcreate@example.com", "Test@1234!");

        // Act
        var response = await _client.PostAsJsonAsync(BasePath, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_WithEmptyUserName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateUserRequest("", "test@example.com", "Test@1234!");

        // Act
        var response = await _client.PostAsJsonAsync(BasePath, request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(BasePath,
            new CreateUserRequest("testuser-getbyid", "testgetbyid@example.com", "Test@1234!"));
        var location = createResponse.Headers.Location!;

        // Act
        var response = await _client.GetAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDetailViewModel>();
        user.ShouldNotBeNull();
        user!.UserName.ShouldBe("testuser-getbyid");
    }

    [Fact]
    public async Task GetById_WithNonExistingUser_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync($"{BasePath}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WithExistingUser_ShouldReturnNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(BasePath,
            new CreateUserRequest("testuser-update-old", "testupdateold@example.com", "Test@1234!"));
        var location = createResponse.Headers.Location!;

        // Send only body fields (no Id) to verify route binding works correctly
        var body = new { UserName = "testuser-update-new", Email = "testupdatenew@example.com" };

        // Act
        var response = await _client.PutAsJsonAsync(location.ToString(), body);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        var updatedUser = await getResponse.Content.ReadFromJsonAsync<UserDetailViewModel>();
        updatedUser!.UserName.ShouldBe("testuser-update-new");
    }

    [Fact]
    public async Task Update_WithNonExistingUser_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.PutAsJsonAsync(
            $"{BasePath}/{Guid.NewGuid()}",
            new { UserName = "does-not-matter", Email = "does@not.matter" });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithExistingUser_ShouldReturnNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync(BasePath,
            new CreateUserRequest("testuser-delete", "testdelete@example.com", "Test@1234!"));
        var location = createResponse.Headers.Location!;

        // Act
        var response = await _client.DeleteAsync(location);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync(location);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithNonExistingUser_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.DeleteAsync($"{BasePath}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
