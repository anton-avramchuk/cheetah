using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Teams.Domain.Entities;

namespace Cheetah.Modules.Teams.Application.Teams;

/// <summary>Общий хвост хендлеров мутации команды: сохранить и опубликовать доменные события.</summary>
internal static class TeamHandlerHelpers
{
    public static async ValueTask SaveAndPublishAsync<TTeam>(
        IRepository<TTeam, Guid> repository, IEventBus eventBus, TTeam team, CancellationToken ct)
        where TTeam : TeamBase
    {
        await repository.SaveChangesAsync(ct);
        foreach (var e in team.DomainEvents)
            await eventBus.PublishAsync(e, ct);
        team.ClearDomainEvents();
    }
}
