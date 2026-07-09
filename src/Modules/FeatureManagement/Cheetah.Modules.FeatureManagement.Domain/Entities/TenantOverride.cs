using Cheetah.Audit;
using Cheetah.Core.Domain;

namespace Cheetah.Modules.FeatureManagement.Domain.Entities;

/// <summary>
/// Переопределение флага для конкретного тенанта (child агрегата). При оценке override тенанта
/// смотрится раньше глобального правила.
/// </summary>
[Auditable]
public sealed class TenantOverride : Entity<Guid>
{
    public Guid FlagId { get; private set; }
    public Guid TenantId { get; private set; }
    public bool Enabled { get; private set; }

    /// <summary>Опциональные правила таргетинга для тенанта в JSON (jsonb); <c>null</c> — простое вкл/выкл.</summary>
    public string? RulesJson { get; private set; }

    private TenantOverride() { }

    public static TenantOverride Create(Guid flagId, Guid tenantId, bool enabled, string? rulesJson)
        => new()
        {
            Id = Guid.NewGuid(),
            FlagId = flagId,
            TenantId = tenantId,
            Enabled = enabled,
            RulesJson = rulesJson
        };
}
