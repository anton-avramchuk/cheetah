using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Modules.NotesTimeline.Application.Timeline;
using Cheetah.Modules.NotesTimeline.Contracts;
using Cheetah.Modules.NotesTimeline.Domain;
using Cheetah.Modules.NotesTimeline.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.NotesTimeline.Application;

/// <summary>
/// Прикладной слой шаблонного модуля NotesTimeline: generic CQRS заметок (закрытые generic-handler'ы
/// регистрирует наследник через <c>AddNotesApplication&lt;…&gt;()</c>) + конкретный чтение ленты
/// (<see cref="GetTimelineQueryHandler"/>). Materializация ленты — через зарегистрированные проекторы
/// (см. <c>AddTimelineProjector</c>).
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CheetahNotesTimelineDomainModule),
    typeof(CheetahNotesTimelineContractsModule),
    typeof(CheetahNotesTimelineDomainEventsModule))]
public partial class CheetahNotesTimelineApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;

        // Чтение ленты не зависит от конкретного типа заметки — регистрируется здесь.
        services.AddScoped<IQueryHandler<GetTimelineQuery, TimelinePageDto>, GetTimelineQueryHandler>();

        RegisterServices(services);
    }
}
