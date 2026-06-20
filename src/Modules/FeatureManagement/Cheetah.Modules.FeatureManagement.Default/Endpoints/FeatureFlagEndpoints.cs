using Cheetah.Modules.FeatureManagement.Api.Endpoints;
using Cheetah.Modules.FeatureManagement.Default.Contracts;

namespace Cheetah.Modules.FeatureManagement.Default.Endpoints;

/// <summary>Конкретные эндпоинты «из коробки» поверх абстрактной базы.</summary>
public sealed class FeatureFlagEndpoints : FeatureFlagEndpointsBase<CreateFeatureFlagRequest, FeatureFlagDto>;
