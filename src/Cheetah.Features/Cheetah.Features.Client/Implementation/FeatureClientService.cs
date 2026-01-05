using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Features.Application.Commands;
using Cheetah.Features.Application.Queries;
using Cheetah.Features.Client.Interfaces;
using Cheetah.Features.Domain.Entities;
using Cheetah.Features.Shared.ViewModels;

namespace Cheetah.Features.Client.Implementation;

/// <summary>
/// Direct implementation of feature client service.
/// Uses IDispatcher for queries and commands.
/// Can be replaced with HTTP client implementation for microservices.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IFeatureClientService))]
public class FeatureClientService : IFeatureClientService
{
    private readonly IDispatcher _dispatcher;

    public FeatureClientService(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async ValueTask<List<FeatureViewModel>> GetAllFeaturesAsync(CancellationToken ct = default)
    {
        var query = new GetAllFeaturesQuery();
        var features = await _dispatcher.QueryAsync<GetAllFeaturesQuery, IReadOnlyList<Feature>>(query, ct);

        return features.Select(MapFeatureToViewModel).ToList();
    }

    public async ValueTask<List<TenantFeatureViewModel>> GetTenantFeaturesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var query = new GetTenantFeaturesQuery(tenantId);
        var tenantFeatures = await _dispatcher.QueryAsync<GetTenantFeaturesQuery, IReadOnlyList<TenantFeature>>(query, ct);

        // Get all features to enrich the view model
        var allFeatures = await GetAllFeaturesAsync(ct);
        var featuresDict = allFeatures.ToDictionary(f => f.Id);

        return tenantFeatures.Select(tf => MapTenantFeatureToViewModel(tf, featuresDict)).ToList();
    }

    public async ValueTask<bool> IsFeatureEnabledAsync(Guid tenantId, string featureId, CancellationToken ct = default)
    {
        var query = new CheckFeatureQuery(tenantId, featureId);
        return await _dispatcher.QueryAsync<CheckFeatureQuery, bool>(query, ct);
    }

    public async ValueTask EnableFeatureAsync(Guid tenantId, string featureId, CancellationToken ct = default)
    {
        var command = new EnableFeatureCommand(tenantId, featureId);
        await _dispatcher.SendAsync(command, ct);
    }

    public async ValueTask DisableFeatureAsync(Guid tenantId, string featureId, CancellationToken ct = default)
    {
        var command = new DisableFeatureCommand(tenantId, featureId);
        await _dispatcher.SendAsync(command, ct);
    }

    private static FeatureViewModel MapFeatureToViewModel(Feature feature)
    {
        return new FeatureViewModel
        {
            Id = feature.Id,
            Name = feature.Name,
            DisplayName = feature.DisplayName,
            Description = feature.Description,
            IsEnabledByDefault = feature.IsEnabledByDefault,
            Group = feature.Group,
            CreatedAt = feature.CreatedAt,
            UpdatedAt = feature.UpdatedAt
        };
    }

    private static TenantFeatureViewModel MapTenantFeatureToViewModel(TenantFeature tenantFeature, Dictionary<string, FeatureViewModel> features)
    {
        var feature = features.GetValueOrDefault(tenantFeature.FeatureId);

        return new TenantFeatureViewModel
        {
            Id = tenantFeature.Id,
            TenantId = tenantFeature.TenantId,
            FeatureId = tenantFeature.FeatureId,
            FeatureName = feature?.Name ?? tenantFeature.FeatureId,
            FeatureDisplayName = feature?.DisplayName ?? tenantFeature.FeatureId,
            FeatureDescription = feature?.Description,
            IsEnabled = tenantFeature.IsEnabled,
            EnabledAt = tenantFeature.EnabledAt,
            DisabledAt = tenantFeature.DisabledAt,
            CreatedAt = tenantFeature.CreatedAt,
            UpdatedAt = tenantFeature.UpdatedAt
        };
    }
}
