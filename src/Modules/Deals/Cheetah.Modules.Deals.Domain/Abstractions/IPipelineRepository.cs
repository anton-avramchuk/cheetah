using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Deals.Domain.Entities;

namespace Cheetah.Modules.Deals.Domain.Abstractions;

/// <summary>
/// Репозиторий воронок с загрузкой стадий (child-entities). Базовый <c>GetByIdAsync</c> не тянет
/// коллекцию <see cref="Pipeline.Stages"/>, поэтому для операций над стадиями используются методы ниже.
/// </summary>
public interface IPipelineRepository : IRepository<Pipeline, Guid>
{
    ValueTask<Pipeline?> GetWithStagesAsync(Guid id, CancellationToken ct = default);
    ValueTask<Pipeline?> GetDefaultWithStagesAsync(CancellationToken ct = default);
    ValueTask<List<Pipeline>> ListWithStagesAsync(bool activeOnly, CancellationToken ct = default);
}
