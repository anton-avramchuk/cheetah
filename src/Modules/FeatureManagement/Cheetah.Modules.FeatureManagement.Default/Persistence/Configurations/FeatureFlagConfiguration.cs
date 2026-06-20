using Cheetah.Modules.FeatureManagement.Default.Entities;
using Cheetah.Modules.FeatureManagement.Infrastructure.Persistence.Configurations;

namespace Cheetah.Modules.FeatureManagement.Default.Persistence.Configurations;

/// <summary>Конкретная EF-конфигурация флага «из коробки» (без доп. полей).</summary>
public sealed class FeatureFlagConfiguration : FeatureFlagConfigurationBase<FeatureFlag>;
