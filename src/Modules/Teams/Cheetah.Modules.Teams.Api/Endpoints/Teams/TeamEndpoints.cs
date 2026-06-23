using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Modules.Teams.Contracts;
using Cheetah.Modules.Teams.Shared;

namespace Cheetah.Modules.Teams.Api.Endpoints.Teams;

/// <summary>
/// Абстрактные шаблоны эндпоинтов команды (команда расширяема). Наследник/хост закрывает
/// generic-параметры своими конкретными Request/Command/Query/Dto/GridViewModel — тогда генератор
/// регистрирует маршруты (как у абстрактных эндпоинтов Identity). Маршруты и имена можно переопределить.
/// </summary>
public abstract class CreateTeamEndpoint<TRequest, TCommand> : CreateCommandEndpoint<TRequest, TCommand>
    where TRequest : CreateTeamRequestBase
    where TCommand : ICommand<Guid>
{
    public override string Route => TeamsConstants.DefaultTeamsRoutePrefix;
    public override string GetByIdRouteName => "GetTeamById";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Teams.Teams");
}

public abstract class GetTeamByIdEndpoint<TRequest, TQuery, TDto>
    : QueryOrNotFoundEndpoint<TRequest, TQuery, TDto, TDto>
    where TRequest : GetTeamByIdRequestBase
    where TQuery : IQuery<TDto?>
    where TDto : TeamDtoBase
{
    public override string Route => $"{TeamsConstants.DefaultTeamsRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("GetTeamById").WithTags("Teams.Teams");
}

public abstract class UpdateTeamEndpoint<TRequest, TCommand> : UpdateCommandEndpoint<TRequest, TCommand>
    where TRequest : UpdateTeamRequestBase
    where TCommand : ICommand
{
    public override string Route => $"{TeamsConstants.DefaultTeamsRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Teams.Teams");
}

public abstract class DeleteTeamEndpoint<TRequest, TCommand> : DeleteCommandEndpoint<TRequest, TCommand>
    where TRequest : DeleteTeamRequestBase
    where TCommand : ICommand
{
    public override string Route => $"{TeamsConstants.DefaultTeamsRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Teams.Teams");
}

public abstract class GetTeamsGridEndpoint<TRequest, TQuery, TGridViewModel>
    : QueryGridEndpoint<TRequest, TQuery, TGridViewModel, TGridViewModel>
    where TRequest : GetTeamsGridRequestBase
    where TQuery : IQuery<GridResult<TGridViewModel>>
    where TGridViewModel : TeamGridViewModelBase
{
    public override string Route => TeamsConstants.DefaultTeamsRoutePrefix;

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Teams.Teams");
}

// ── Состав команды (мутации членства) ──────────────────────────────────────────────────────────

public abstract class AddTeamMemberEndpoint<TRequest, TCommand> : CommandEndpoint<TRequest, TCommand>
    where TRequest : AddTeamMemberRequestBase
    where TCommand : ICommand
{
    public override string Route => $"{TeamsConstants.DefaultTeamsRoutePrefix}/{{id:guid}}/members";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Teams.Teams");
}

public abstract class ChangeTeamMemberRoleEndpoint<TRequest, TCommand> : UpdateCommandEndpoint<TRequest, TCommand>
    where TRequest : ChangeTeamMemberRoleRequestBase
    where TCommand : ICommand
{
    public override string Route => $"{TeamsConstants.DefaultTeamsRoutePrefix}/{{id:guid}}/members/{{memberId:guid}}";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Teams.Teams");
}

public abstract class RemoveTeamMemberEndpoint<TRequest, TCommand> : DeleteCommandEndpoint<TRequest, TCommand>
    where TRequest : RemoveTeamMemberRequestBase
    where TCommand : ICommand
{
    public override string Route => $"{TeamsConstants.DefaultTeamsRoutePrefix}/{{id:guid}}/members/{{memberId:guid}}";

    protected override void Configure(EndpointConfiguration config) => config.WithTags("Teams.Teams");
}
