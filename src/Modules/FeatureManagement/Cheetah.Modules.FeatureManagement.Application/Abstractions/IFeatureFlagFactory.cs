using Cheetah.Modules.FeatureManagement.Contracts;
using Cheetah.Modules.FeatureManagement.Domain.Entities;

namespace Cheetah.Modules.FeatureManagement.Application.Abstractions;

/// <summary>
/// Фабрика конкретного флага. Реализуется наследником — он знает, как сконструировать свой
/// <c>sealed</c>-тип (включая доп. поля) через <c>InitializeCore</c>. Так generic-handler создаёт флаг,
/// не зная конкретного типа.
/// </summary>
public interface IFeatureFlagFactory<out TFlag, in TCreateRequest>
    where TFlag : FeatureFlagBase
    where TCreateRequest : CreateFeatureFlagRequestBase
{
    /// <summary>Создать флаг из запроса админки.</summary>
    TFlag Create(TCreateRequest request);

    /// <summary>Создать флаг из дескриптора реестра (registry/sync) — метаданные без таргетинга.</summary>
    TFlag CreateFromDescriptor(FeatureDefinitionDescriptor descriptor);
}
