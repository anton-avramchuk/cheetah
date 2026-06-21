namespace Cheetah.Core.DataAccess.Abstractions;

public interface IConnectionStringResolver
{
    Task<string> ResolveAsync(string? connectionStringName = null);

    /// <summary>
    /// Синхронное разрешение строки подключения. Нужен EF Core: фабрика
    /// <c>DbContextOptions</c> вызывается синхронно при создании DbContext, поэтому
    /// горячий путь не может быть асинхронным.
    /// <para>
    /// Реализация по умолчанию блокирует <see cref="ResolveAsync"/> и безопасна только
    /// для резолверов, завершающихся синхронно (как <c>DefaultConnectionStringResolver</c>).
    /// Кастомные резолверы, делающие реальный async-I/O, ОБЯЗАНЫ переопределить этот метод
    /// собственной синхронной реализацией, чтобы не блокировать пул потоков.
    /// </para>
    /// </summary>
    string Resolve(string? connectionStringName = null)
        => ResolveAsync(connectionStringName).GetAwaiter().GetResult();
}
