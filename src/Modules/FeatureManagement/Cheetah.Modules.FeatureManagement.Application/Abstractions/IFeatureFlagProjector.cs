using Cheetah.Modules.FeatureManagement.Contracts;
using Cheetah.Modules.FeatureManagement.Domain.Entities;

namespace Cheetah.Modules.FeatureManagement.Application.Abstractions;

/// <summary>
/// Проектор флага в конкретный DTO наследника (включая доп. поля). Заменяет Mapster, чтобы
/// расширения не требовали скрытой регистрации.
/// </summary>
public interface IFeatureFlagProjector<in TFlag, out TDto>
    where TFlag : FeatureFlagBase
    where TDto : FeatureFlagDtoBase
{
    TDto ToDto(TFlag flag);
}
