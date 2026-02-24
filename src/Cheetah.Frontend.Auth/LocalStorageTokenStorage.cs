using Cheetah.Core.DependencyInjection;
using Microsoft.JSInterop;

namespace Cheetah.Frontend.Auth;

[Export(LifetimeType.Scoped, typeof(ITokenStorage))]
public sealed class LocalStorageTokenStorage : ITokenStorage
{
    private const string TokenKey = "crm_auth_token";
    private readonly IJSRuntime _js;

    public LocalStorageTokenStorage(IJSRuntime js) => _js = js;

    public async ValueTask<string?> GetTokenAsync(CancellationToken ct = default)
        => await _js.InvokeAsync<string?>("localStorage.getItem", ct, TokenKey);

    public async ValueTask SetTokenAsync(string token, CancellationToken ct = default)
        => await _js.InvokeVoidAsync("localStorage.setItem", ct, TokenKey, token);

    public async ValueTask RemoveTokenAsync(CancellationToken ct = default)
        => await _js.InvokeVoidAsync("localStorage.removeItem", ct, TokenKey);
}
