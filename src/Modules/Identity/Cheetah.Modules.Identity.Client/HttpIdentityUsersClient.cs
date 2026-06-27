using System.Net.Http.Json;
using Cheetah.Contracts.Responses;

namespace Cheetah.Modules.Identity.Client;

public sealed class HttpIdentityUsersClient(HttpClient http) : IIdentityUsersClient
{
    public async ValueTask<IReadOnlyList<IdentityUserSummary>> GetUsersAsync(CancellationToken ct = default)
    {
        // PageSize=0 => Identity отдаёт все записи одним ответом. Десериализуем в конкретный
        // снимок: расширенные поля grid-ViewModel хоста при этом отбрасываются.
        var result = await http.GetFromJsonAsync<GridResult<IdentityUserSummary>>("api/users?Page=1&PageSize=0", ct);
        return result?.Data?.ToArray() ?? Array.Empty<IdentityUserSummary>();
    }
}
