using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Domain.Entities;

namespace Cheetah.Modules.FeatureManagement.Default.Entities;

/// <summary>
/// Конкретный флаг «из коробки». Наследник может вместо <c>.Default</c> объявить свой
/// <c>sealed class FeatureFlag : FeatureFlagBase</c> с доп. полями — это точка расширяемости сущности.
/// </summary>
public sealed class FeatureFlag : FeatureFlagBase
{
    private FeatureFlag() { }

    public static FeatureFlag Create(string key, string name, string ownerService,
        FeatureValueType valueType, string? description)
    {
        var flag = new FeatureFlag();
        flag.InitializeCore(Guid.NewGuid(), key, name, ownerService, valueType, description);
        return flag;
    }
}
