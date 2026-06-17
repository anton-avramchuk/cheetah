using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Modules.NotesTimeline.Application.Abstractions;
using Cheetah.Modules.NotesTimeline.Application.Notes;
using Cheetah.Modules.NotesTimeline.Application.Timeline;
using Cheetah.Modules.NotesTimeline.Contracts;
using Cheetah.Modules.NotesTimeline.Domain.Abstractions;
using Cheetah.Modules.NotesTimeline.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.NotesTimeline.Application.Extensions;

public static class NotesTimelineApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует фабрику, проектор и закрытые generic CQRS-handler'ы конкретной реализации заметок.
    /// Вызывается из прикладного модуля наследника после <c>AddNotesTimelineInfrastructure</c>.
    /// </summary>
    public static IServiceCollection AddNotesApplication<TNote, TCreateRequest, TUpdateRequest, TDto, TFactory, TProjector>(
        this IServiceCollection services)
        where TNote : NoteBase
        where TCreateRequest : CreateNoteRequestBase
        where TUpdateRequest : UpdateNoteRequestBase
        where TDto : NoteDtoBase
        where TFactory : class, INoteFactory<TNote, TCreateRequest>
        where TProjector : class, INoteProjector<TNote, TDto>
    {
        services.AddScoped<INoteFactory<TNote, TCreateRequest>, TFactory>();
        services.AddScoped<INoteProjector<TNote, TDto>, TProjector>();

        services.AddScoped<ICommandHandler<CreateNoteCommand<TCreateRequest>, Guid>,
            CreateNoteCommandHandler<TNote, TCreateRequest>>();
        services.AddScoped<ICommandHandler<UpdateNoteCommand<TUpdateRequest>>,
            UpdateNoteCommandHandler<TNote, TUpdateRequest>>();
        services.AddScoped<ICommandHandler<PinNoteCommand>, PinNoteCommandHandler<TNote>>();
        services.AddScoped<ICommandHandler<UnpinNoteCommand>, UnpinNoteCommandHandler<TNote>>();
        services.AddScoped<ICommandHandler<RemoveNoteCommand>, RemoveNoteCommandHandler<TNote>>();
        services.AddScoped<IQueryHandler<GetNotesByEntityQuery<TDto>, IReadOnlyList<TDto>>,
            GetNotesByEntityQueryHandler<TNote, TDto>>();

        return services;
    }

    /// <summary>
    /// Регистрирует проектор события в строку ленты (точка расширяемости Timeline). Вызывается
    /// модулем-источником события. Несколько проекторов на один тип события допустимы.
    /// </summary>
    public static IServiceCollection AddTimelineProjector<TEvent, TProjector>(this IServiceCollection services)
        where TEvent : EventBase
        where TProjector : class, ITimelineProjector<TEvent>
    {
        services.AddScoped<ITimelineProjector<TEvent>, TProjector>();
        services.AddScoped<TimelineProjectionService<TEvent>>();
        return services;
    }
}
