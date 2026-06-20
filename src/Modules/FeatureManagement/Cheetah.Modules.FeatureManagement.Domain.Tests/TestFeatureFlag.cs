using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Domain.Entities;

namespace Cheetah.Modules.FeatureManagement.Domain.Tests;

/// <summary>
/// Конкретный наследник <see cref="FeatureFlagBase"/> с доп. полем — проверяет, что абстрактный
/// шаблон расширяется наследником (как <c>Activity : ActivityBase</c>).
/// </summary>
internal sealed class TestFeatureFlag : FeatureFlagBase
{
    public string? OwnerTeam { get; private set; }

    private TestFeatureFlag() { }

    public static TestFeatureFlag Create(string key, string name, string ownerService,
        FeatureValueType valueType = FeatureValueType.Bool, string? description = null, string? ownerTeam = null)
    {
        var flag = new TestFeatureFlag();
        flag.InitializeCore(Guid.NewGuid(), key, name, ownerService, valueType, description);
        flag.OwnerTeam = ownerTeam;
        return flag;
    }
}
