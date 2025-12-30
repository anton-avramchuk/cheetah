using System.Net;
using System.Net.Http.Json;
using Cheetah.Core.DependencyInjection;
using Cheetah.Identity.Shared.Requests;
using Cheetah.Identity.Shared.ViewModels;

namespace Cheetah.Identity.Client;

/// <summary>
/// HTTP-based client for Identity API
/// </summary>
[Export(LifetimeType.Scoped, typeof(IIdentityClient))]
public class IdentityClient : IIdentityClient
{
    private readonly HttpClient _httpClient;

    public IdentityClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("CheetahAPI");
    }

    // Authentication

    public async ValueTask<UserViewModel?> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/identity/register", request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserViewModel>(ct);
    }

    public async ValueTask ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"/api/identity/users/{userId}/change-password", request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async ValueTask ConfirmEmailAsync(Guid userId, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsync($"/api/identity/confirm-email/{userId}", null, ct);
        response.EnsureSuccessStatusCode();
    }

    // Users

    public async ValueTask<UserViewModel?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"/api/identity/users/{userId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserViewModel>(ct);
    }

    public async ValueTask<UserViewModel?> GetUserByEmailAsync(string email, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"/api/identity/users/email/{email}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserViewModel>(ct);
    }

    public async ValueTask<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"/api/identity/users/{userId}/permissions", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<string>>(ct) ?? new List<string>();
    }

    // Roles

    public async ValueTask<Guid> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/identity/roles", request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RoleCreatedResponse>(ct);
        return result!.Id;
    }

    public async ValueTask<List<RoleViewModel>> GetAllRolesAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("/api/identity/roles", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<RoleViewModel>>(ct) ?? new List<RoleViewModel>();
    }

    // Permissions

    public async ValueTask AddPermissionToRoleAsync(Guid roleId, string permission, CancellationToken ct = default)
    {
        var request = new AddPermissionRequest { Permission = permission };
        var response = await _httpClient.PostAsJsonAsync($"/api/identity/roles/{roleId}/permissions", request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async ValueTask AddPermissionToUserAsync(Guid userId, string permission, CancellationToken ct = default)
    {
        var request = new AddPermissionRequest { Permission = permission };
        var response = await _httpClient.PostAsJsonAsync($"/api/identity/users/{userId}/permissions", request, ct);
        response.EnsureSuccessStatusCode();
    }

    public async ValueTask AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsync($"/api/identity/users/{userId}/roles/{roleId}", null, ct);
        response.EnsureSuccessStatusCode();
    }

    // Helper response class for CreateRole
    private class RoleCreatedResponse
    {
        public Guid Id { get; set; }
    }
}
