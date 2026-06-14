using System.Net.Http.Json;
using Cheetah.Contracts.Responses;
using Cheetah.Modules.Identity.Contracts.Response;

namespace Cheetah.Modules.Identity.Client;

public sealed class HttpIdentityUsersClient : IIdentityUsersClient
{
    private readonly HttpClient _http;

    public HttpIdentityUsersClient(HttpClient http) => _http = http;

    public async ValueTask<IReadOnlyList<UserGridViewModel>> GetUsersAsync(CancellationToken ct = default)
    {
        // PageSize=0 => Identity отдаёт все записи одним ответом
        var result = await _http.GetFromJsonAsync<GridResult<UserGridViewModel>>("api/users?Page=1&PageSize=0", ct);
        return result?.Data?.ToArray() ?? Array.Empty<UserGridViewModel>();
    }
}
