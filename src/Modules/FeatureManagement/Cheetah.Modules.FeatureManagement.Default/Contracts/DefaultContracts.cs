using Cheetah.Modules.FeatureManagement.Contracts;

namespace Cheetah.Modules.FeatureManagement.Default.Contracts;

/// <summary>Конкретный запрос создания флага «из коробки».</summary>
public sealed record CreateFeatureFlagRequest : CreateFeatureFlagRequestBase;

/// <summary>Конкретный ViewModel флага «из коробки».</summary>
public sealed record FeatureFlagDto : FeatureFlagDtoBase;
