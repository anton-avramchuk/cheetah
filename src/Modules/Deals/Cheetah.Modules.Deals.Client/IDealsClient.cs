using Cheetah.Modules.Deals.Contracts;

namespace Cheetah.Modules.Deals.Client;

/// <summary>
/// HTTP-клиент к Deals.Api для server-to-server интеграции (например, конвертация Lead → Deal):
/// создание сделки и чтение карточки.
/// </summary>
public interface IDealsClient
{
    /// <summary>Создать сделку. Возвращает идентификатор созданной сделки.</summary>
    ValueTask<Guid> CreateDealAsync(CreateDealRequest request, CancellationToken ct = default);

    /// <summary>Получить сделку по идентификатору (null — если не найдена).</summary>
    ValueTask<DealDto?> GetByIdAsync(Guid dealId, CancellationToken ct = default);
}
