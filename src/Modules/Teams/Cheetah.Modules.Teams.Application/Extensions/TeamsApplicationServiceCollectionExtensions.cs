using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Modules.Teams.Application.Abstractions;
using Cheetah.Modules.Teams.Application.Teams;
using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Teams.Application.Extensions;

public static class TeamsApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует фабрику, проектор и закрытые generic CQRS-handler'ы команды конкретной реализации
    /// (включая мутации состава, удаление и грид). Хендлеры ролей и участников не зависят от типа
    /// команды и регистрируются генератором по <c>[Export]</c>. Вызывается из прикладного модуля
    /// наследника после <c>AddTeamsInfrastructure</c>.
    /// </summary>
    public static IServiceCollection AddTeamsApplication<
        TTeam, TCreateRequest, TUpdateRequest, TDto, TGridViewModel, TFactory, TProjector>(
        this IServiceCollection services)
        where TTeam : TeamBase
        where TCreateRequest : CreateTeamRequestBase
        where TUpdateRequest : UpdateTeamRequestBase
        where TDto : TeamDtoBase
        where TGridViewModel : TeamGridViewModelBase
        where TFactory : class, ITeamFactory<TTeam, TCreateRequest>
        where TProjector : class, ITeamProjector<TTeam, TDto>
    {
        services.AddScoped<ITeamFactory<TTeam, TCreateRequest>, TFactory>();
        services.AddScoped<ITeamProjector<TTeam, TDto>, TProjector>();

        services.AddScoped<ICommandHandler<CreateTeamCommand<TCreateRequest>, Guid>,
            CreateTeamCommandHandler<TTeam, TCreateRequest>>();
        services.AddScoped<ICommandHandler<UpdateTeamCommand<TUpdateRequest>>,
            UpdateTeamCommandHandler<TTeam, TUpdateRequest>>();
        services.AddScoped<ICommandHandler<DeactivateTeamCommand>,
            DeactivateTeamCommandHandler<TTeam>>();
        services.AddScoped<ICommandHandler<ActivateTeamCommand>,
            ActivateTeamCommandHandler<TTeam>>();
        services.AddScoped<ICommandHandler<DeleteTeamCommand>,
            DeleteTeamCommandHandler<TTeam>>();

        services.AddScoped<ICommandHandler<AddTeamMemberCommand>,
            AddTeamMemberCommandHandler<TTeam>>();
        services.AddScoped<ICommandHandler<ChangeTeamMemberRoleCommand>,
            ChangeTeamMemberRoleCommandHandler<TTeam>>();
        services.AddScoped<ICommandHandler<RemoveTeamMemberCommand>,
            RemoveTeamMemberCommandHandler<TTeam>>();

        services.AddScoped<IQueryHandler<GetTeamByIdQuery<TDto>, TDto?>,
            GetTeamByIdQueryHandler<TTeam, TDto>>();
        services.AddScoped<IQueryHandler<ListTeamsQuery<TDto>, IReadOnlyList<TDto>>,
            ListTeamsQueryHandler<TTeam, TDto>>();
        services.AddScoped<IQueryHandler<GetTeamsGridQuery<TGridViewModel>, GridResult<TGridViewModel>>,
            GetTeamsGridQueryHandler<TTeam, TGridViewModel>>();

        return services;
    }
}
