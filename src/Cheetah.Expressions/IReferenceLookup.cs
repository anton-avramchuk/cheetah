namespace Cheetah.Expressions;

/// <summary>
/// Источник для оператора reference_exists: проверка существования сущности по id в справочнике.
/// В монолите реализуется обращением к локальному репозиторию, в микросервисах — к client-библиотеке
/// соответствующего сервиса (MasterData.Client, Customer.Client, ...).
///
/// Если не зарегистрирован — оператор reference_exists возвращает false.
/// </summary>
public interface IReferenceLookup
{
    /// <summary>
    /// Проверить, существует ли запись с указанным id в справочнике с указанным именем.
    /// </summary>
    /// <param name="dictionaryName">Логическое имя справочника, например "Cities" или "Counterparties".</param>
    /// <param name="id">Идентификатор сущности.</param>
    ValueTask<bool> ExistsAsync(string dictionaryName, object id, CancellationToken ct = default);
}
