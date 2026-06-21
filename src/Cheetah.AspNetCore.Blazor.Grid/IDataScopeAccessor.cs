namespace Cheetah.AspNetCore.Blazor.Grid;

/// <summary>
/// Резолвит <see cref="DataScope"/> текущего пользователя (id + право видеть все записи).
/// Реализация живёт в модуле Identity; downstream-модули инжектят только этот интерфейс.
/// </summary>
public interface IDataScopeAccessor
{
    Task<DataScope> ResolveAsync(CancellationToken ct = default);
}
