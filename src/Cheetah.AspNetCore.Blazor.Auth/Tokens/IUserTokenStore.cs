using Cheetah.AspNetCore.Blazor.Auth.Abstractions;

namespace Cheetah.AspNetCore.Blazor.Auth.Tokens;

/// <summary>
/// Серверное хранилище токенов пользователя, ключ — id серверной сессии (claim
/// <see cref="BffClaimTypes.SessionId"/> в cookie). Дефолтная реализация — in-memory;
/// для масштабирования на несколько инстансов замените на Redis-реализацию.
/// </summary>
public interface IUserTokenStore
{
    ValueTask StoreAsync(string sessionId, BffTokenSet tokens, CancellationToken ct = default);

    ValueTask<BffTokenSet?> GetAsync(string sessionId, CancellationToken ct = default);

    ValueTask RemoveAsync(string sessionId, CancellationToken ct = default);
}
