using Cheetah.Core.CQRS;
using Cheetah.Modules.FeatureManagement.Application.Abstractions;
using Cheetah.Modules.FeatureManagement.Application.Commands;
using Cheetah.Modules.FeatureManagement.Application.Queries;
using Cheetah.Modules.FeatureManagement.Contracts;
using Cheetah.Modules.FeatureManagement.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.FeatureManagement.Application.Extensions;

public static class FeatureManagementApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует фабрику, проектор и закрытые generic CQRS-handler'ы конкретной реализации
    /// FeatureManagement. Вызывается из прикладного модуля наследника / <c>.Default</c> после
    /// <c>AddFeatureManagementInfrastructure</c>. (Батч-<c>EvaluateFeaturesQuery</c> не зависит от типа
    /// флага и регистрируется генератором через <c>[Export]</c>.)
    /// </summary>
    public static IServiceCollection AddFeatureManagementApplication<TFlag, TCreateRequest, TDto, TFactory, TProjector>(
        this IServiceCollection services)
        where TFlag : FeatureFlagBase
        where TCreateRequest : CreateFeatureFlagRequestBase
        where TDto : FeatureFlagDtoBase
        where TFactory : class, IFeatureFlagFactory<TFlag, TCreateRequest>
        where TProjector : class, IFeatureFlagProjector<TFlag, TDto>
    {
        services.AddScoped<IFeatureFlagFactory<TFlag, TCreateRequest>, TFactory>();
        services.AddScoped<IFeatureFlagProjector<TFlag, TDto>, TProjector>();

        services.AddScoped<ICommandHandler<CreateFeatureFlagCommand<TCreateRequest>, Guid>,
            CreateFeatureFlagCommandHandler<TFlag, TCreateRequest>>();
        services.AddScoped<ICommandHandler<EnableFeatureFlagCommand>, EnableFeatureFlagCommandHandler<TFlag>>();
        services.AddScoped<ICommandHandler<DisableFeatureFlagCommand>, DisableFeatureFlagCommandHandler<TFlag>>();
        services.AddScoped<ICommandHandler<SetTargetingCommand>, SetTargetingCommandHandler<TFlag>>();
        services.AddScoped<ICommandHandler<SetTenantOverrideCommand>, SetTenantOverrideCommandHandler<TFlag>>();
        services.AddScoped<ICommandHandler<SetParentFeatureFlagCommand>, SetParentFeatureFlagCommandHandler<TFlag>>();
        services.AddScoped<ICommandHandler<SyncFeatureRegistryCommand>,
            SyncFeatureRegistryCommandHandler<TFlag, TCreateRequest>>();

        services.AddScoped<IQueryHandler<GetFeatureFlagByKeyQuery<TDto>, TDto?>,
            GetFeatureFlagByKeyQueryHandler<TFlag, TDto>>();
        services.AddScoped<IQueryHandler<ListFeatureFlagsQuery<TDto>, IReadOnlyList<TDto>>,
            ListFeatureFlagsQueryHandler<TFlag, TDto>>();

        return services;
    }
}
