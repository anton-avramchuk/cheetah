using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.Client;
using Cheetah.Modules.Tags.Domain.Abstractions;

namespace Cheetah.Modules.Tags.Infrastructure.Identity;

/// <summary>
/// Реализация порта <see cref="IIdentityUserDirectory"/> поверх клиента Identity
/// (<see cref="IIdentityUsersClient"/>). Маппит контракт Identity в снимок Tags-реплики.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IIdentityUserDirectory))]
public sealed class IdentityClientUserDirectory : IIdentityUserDirectory
{
    private readonly IIdentityUsersClient _client;

    public IdentityClientUserDirectory(IIdentityUsersClient client) => _client = client;

    public async ValueTask<IReadOnlyList<UserDirectoryEntry>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _client.GetUsersAsync(ct);
        return users.Select(u => new UserDirectoryEntry(u.Id, u.UserName)).ToArray();
    }
}
