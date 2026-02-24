namespace Cheetah.Frontend.Auth;

public interface ITokenStorage
{
    ValueTask<string?> GetTokenAsync(CancellationToken ct = default);
    ValueTask SetTokenAsync(string token, CancellationToken ct = default);
    ValueTask RemoveTokenAsync(CancellationToken ct = default);
}
