using System.Collections.Concurrent;
using Cheetah.AspNetCore.Blazor.Auth.Abstractions;

namespace Cheetah.AspNetCore.Blazor.Auth.Tokens;

/// <summary>
/// In-memory реализация <see cref="IUserTokenStore"/> для single-instance BFF. Записи удаляются
/// при logout и при неуспешном refresh.
/// <para>
/// НЕ регистрируется автоматически — реализацию <see cref="IUserTokenStore"/> приложение выбирает
/// само: либо <c>services.AddSingleton&lt;IUserTokenStore, InMemoryUserTokenStore&gt;()</c>,
/// либо подключите модуль <c>CrmBlazorAuthRedisModule</c> (Redis) для нескольких инстансов.
/// </para>
/// </summary>
public sealed class InMemoryUserTokenStore : IUserTokenStore
{
    private readonly ConcurrentDictionary<string, BffTokenSet> _tokens = new();

    public ValueTask StoreAsync(string sessionId, BffTokenSet tokens, CancellationToken ct = default)
    {
        _tokens[sessionId] = tokens;
        return ValueTask.CompletedTask;
    }

    public ValueTask<BffTokenSet?> GetAsync(string sessionId, CancellationToken ct = default)
        => ValueTask.FromResult(_tokens.TryGetValue(sessionId, out var t) ? t : null);

    public ValueTask RemoveAsync(string sessionId, CancellationToken ct = default)
    {
        _tokens.TryRemove(sessionId, out _);
        return ValueTask.CompletedTask;
    }
}
