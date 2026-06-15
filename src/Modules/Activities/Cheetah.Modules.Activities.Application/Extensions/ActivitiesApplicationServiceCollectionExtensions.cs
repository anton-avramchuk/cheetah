using Cheetah.Core.CQRS;
using Cheetah.Modules.Activities.Application.Abstractions;
using Cheetah.Modules.Activities.Application.Activities;
using Cheetah.Modules.Activities.Contracts;
using Cheetah.Modules.Activities.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Activities.Application.Extensions;

public static class ActivitiesApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует фабрику, проектор и закрытые generic CQRS-handler'ы конкретной реализации.
    /// Вызывается из прикладного модуля наследника после <c>AddActivitiesInfrastructure</c>.
    /// </summary>
    public static IServiceCollection AddActivitiesApplication<TActivity, TCreateRequest, TUpdateRequest, TDto, TFactory, TProjector>(
        this IServiceCollection services)
        where TActivity : ActivityBase
        where TCreateRequest : CreateActivityRequestBase
        where TUpdateRequest : UpdateActivityRequestBase
        where TDto : ActivityDtoBase
        where TFactory : class, IActivityFactory<TActivity, TCreateRequest>
        where TProjector : class, IActivityProjector<TActivity, TDto>
    {
        services.AddScoped<IActivityFactory<TActivity, TCreateRequest>, TFactory>();
        services.AddScoped<IActivityProjector<TActivity, TDto>, TProjector>();

        services.AddScoped<ICommandHandler<CreateActivityCommand<TCreateRequest>, Guid>,
            CreateActivityCommandHandler<TActivity, TCreateRequest>>();
        services.AddScoped<ICommandHandler<UpdateActivityCommand<TUpdateRequest>>,
            UpdateActivityCommandHandler<TActivity, TUpdateRequest>>();
        services.AddScoped<ICommandHandler<StartActivityCommand>,
            StartActivityCommandHandler<TActivity>>();
        services.AddScoped<ICommandHandler<CompleteActivityCommand>,
            CompleteActivityCommandHandler<TActivity>>();
        services.AddScoped<ICommandHandler<CancelActivityCommand>,
            CancelActivityCommandHandler<TActivity>>();
        services.AddScoped<ICommandHandler<ReassignActivityCommand>,
            ReassignActivityCommandHandler<TActivity>>();
        services.AddScoped<IQueryHandler<GetActivityByIdQuery<TDto>, TDto?>,
            GetActivityByIdQueryHandler<TActivity, TDto>>();
        services.AddScoped<IQueryHandler<ListActivitiesQuery<TDto>, IReadOnlyList<TDto>>,
            ListActivitiesQueryHandler<TActivity, TDto>>();

        return services;
    }
}
