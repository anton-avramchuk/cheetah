using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Modules.NotesTimeline.Api.Endpoints;
using Cheetah.Modules.NotesTimeline.Application;
using Cheetah.Modules.NotesTimeline.Contracts;

namespace Cheetah.Modules.NotesTimeline.Api;

/// <summary>
/// Абстрактный базовый Api-модуль: маппит эндпоинты заметок (generic, закрываются наследником) и
/// конкретные эндпоинты ленты в <see cref="OnApplicationInitialization"/>. Наследник закрывает
/// generic-параметры своими типами эндпоинтов/Contracts:
/// <code>
/// public sealed class AppNotesTimelineApiModule
///     : CheetahNotesTimelineApiModuleBase&lt;AppNoteEndpoints, CreateNoteRequest, UpdateNoteRequest, NoteDto&gt; { }
/// </code>
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CheetahNotesTimelineApplicationModule),
    typeof(CheetahNotesTimelineContractsModule))]
public abstract class CheetahNotesTimelineApiModuleBase<TEndpoints, TCreateRequest, TUpdateRequest, TDto> : CrmModule
    where TEndpoints : NoteEndpointsBase<TCreateRequest, TUpdateRequest, TDto>, new()
    where TCreateRequest : CreateNoteRequestBase
    where TUpdateRequest : UpdateNoteRequestBase
    where TDto : NoteDtoBase
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routes = context.GetRouteBuilder();
        new TEndpoints().Map(routes);
        new TimelineEndpoints().Map(routes);
    }
}
