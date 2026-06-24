namespace Cheetah.Backend.ServiceAuth;

/// <summary>
/// Поставляет действующий сервисный токен (machine-to-machine), кэшируя его и обновляя
/// до истечения. Используется <see cref="ServiceTokenHandler"/> для исходящих вызовов.
/// </summary>
public interface IServiceTokenProvider
{
    ValueTask<string> GetTokenAsync(CancellationToken ct = default);
}
