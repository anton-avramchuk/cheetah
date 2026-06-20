using Cheetah.Core.Cache;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Domain.Entities;
using Cheetah.Modules.FeatureManagement.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.FeatureManagement.Infrastructure.Persistence;

/// <summary>
/// Реализация порта <see cref="IFeatureDefinitionProvider"/> поверх БД + кэша (горячий путь
/// монолита / сервиса FeatureManagement). Чтение — из <see cref="ICacheService"/>, БД — только на
/// cache-miss; инвалидация — по событию (<see cref="FeatureCacheInvalidator"/>).
/// </summary>
public sealed class CachedFeatureDefinitionProvider<TFlag> : IFeatureDefinitionProvider
    where TFlag : FeatureFlagBase
{
    private readonly IRepository<TFlag, Guid> _flags;
    private readonly ICacheService _cache;

    public CachedFeatureDefinitionProvider(IRepository<TFlag, Guid> flags, ICacheService cache)
    {
        _flags = flags;
        _cache = cache;
    }

    public ValueTask<FeatureDefinition?> GetAsync(string featureKey, Guid? tenantId, CancellationToken ct = default)
        => _cache.GetOrSetAsync(
            FeatureCacheKeys.Definition(featureKey, tenantId),
            async () => await LoadAsync(featureKey, tenantId, ct),
            FeatureManagementConstants.CacheTtl,
            ct);

    public async ValueTask<IReadOnlyList<FeatureDefinition>> GetAllAsync(Guid? tenantId, CancellationToken ct = default)
    {
        var flags = await Query().Where(f => f.IsActive).ToListAsync(ct);
        return flags.Select(f => FeatureDefinitionMapper.ToDefinition(f, tenantId)).ToArray();
    }

    private async ValueTask<FeatureDefinition?> LoadAsync(string featureKey, Guid? tenantId, CancellationToken ct)
    {
        var flag = await Query().FirstOrDefaultAsync(f => f.Key == featureKey, ct);
        return flag is null ? null : FeatureDefinitionMapper.ToDefinition(flag, tenantId);
    }

    private IQueryable<TFlag> Query()
        => _flags.AsNoTrackingQueryable()
            .Include(f => f.Rules)
            .Include(f => f.Variants)
            .Include(f => f.Overrides);
}
