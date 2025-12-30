using Cheetah.Identity.Shared.Requests;
using Cheetah.Identity.Shared.Responses;
using Cheetah.Identity.Shared.ViewModels;

namespace Cheetah.Identity.Client;

/// <summary>
/// Client for Identity API (backend-to-backend communication)
/// </summary>
public interface IIdentityClient
{
    // Authentication
    ValueTask<UserViewModel?> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default);
    ValueTask ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
    ValueTask ConfirmEmailAsync(Guid userId, CancellationToken ct = default);

    // Users
    ValueTask<UserViewModel?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    ValueTask<UserViewModel?> GetUserByEmailAsync(string email, CancellationToken ct = default);
    ValueTask<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default);

    // Roles
    ValueTask<Guid> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default);
    ValueTask<List<RoleViewModel>> GetAllRolesAsync(CancellationToken ct = default);

    // Permissions
    ValueTask AddPermissionToRoleAsync(Guid roleId, string permission, CancellationToken ct = default);
    ValueTask AddPermissionToUserAsync(Guid userId, string permission, CancellationToken ct = default);
    ValueTask AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken ct = default);
}
