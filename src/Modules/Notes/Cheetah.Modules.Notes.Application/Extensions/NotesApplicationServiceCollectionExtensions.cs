using Cheetah.Core.CQRS;
using Cheetah.Modules.Notes.Application.Abstractions;
using Cheetah.Modules.Notes.Application.Notes;
using Cheetah.Modules.Notes.Contracts;
using Cheetah.Modules.Notes.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Modules.Notes.Application.Extensions;

public static class NotesApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует фабрику, проектор и закрытые generic CQRS-handler'ы конкретной реализации заметок.
    /// Вызывается из прикладного модуля наследника после <c>AddNotesInfrastructure</c>.
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

        services.AddScoped<IQueryHandler<GetNoteByIdQuery<TDto>, TDto?>,
            GetNoteByIdQueryHandler<TNote, TDto>>();
        services.AddScoped<IQueryHandler<GetNotesByEntityQuery<TDto>, IReadOnlyList<TDto>>,
            GetNotesByEntityQueryHandler<TNote, TDto>>();
        services.AddScoped<IQueryHandler<GetNoteRepliesQuery<TDto>, IReadOnlyList<TDto>>,
            GetNoteRepliesQueryHandler<TNote, TDto>>();

        return services;
    }
}
