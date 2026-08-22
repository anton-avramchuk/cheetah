using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Core.CQRS;
using Cheetah.Modules.Notes.Contracts;
using Cheetah.Modules.Notes.Shared;

namespace Cheetah.Modules.Notes.Api.Endpoints;

/// <summary>
/// Абстрактные шаблоны декларативных эндпоинтов заметки (заметка расширяема). Наследник/хост
/// закрывает generic-параметры своими конкретными Request/Command/Query/Dto — тогда генератор
/// <c>Cheetah.Generators.Endpoints</c> регистрирует маршруты в его Api-модуле. Маршруты, имена и
/// метаданные можно переопределить.
/// <para>
/// Готовый закрытый набор «из коробки» — в сборке <c>Cheetah.Modules.Notes.Default</c>.
/// </para>
/// </summary>
public abstract class CreateNoteEndpoint<TRequest, TCommand> : CreateCommandEndpoint<TRequest, TCommand>
    where TRequest : CreateNoteRequestBase
    where TCommand : ICommand<Guid>
{
    public override string Route => NotesConstants.DefaultNotesRoutePrefix;
    public override string GetByIdRouteName => "NotesGetNoteById";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("NotesCreateNote").WithTags("Notes");
}

/// <summary>Заметка по идентификатору; 404, если не найдена или удалена.</summary>
public abstract class GetNoteByIdEndpoint<TRequest, TQuery, TDto>
    : QueryOrNotFoundEndpoint<TRequest, TQuery, TDto, TDto>
    where TRequest : GetNoteByIdRequestBase
    where TQuery : IQuery<TDto?>
    where TDto : NoteDtoBase
{
    public override string Route => $"{NotesConstants.DefaultNotesRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("NotesGetNoteById").WithTags("Notes");
}

/// <summary>Заметки сущности <c>(entityType, entityId)</c>: закреплённые первыми, затем новые сверху.</summary>
public abstract class GetNotesByEntityEndpoint<TRequest, TQuery, TDto>
    : QueryCollectionEndpoint<TRequest, TQuery, TDto, TDto>
    where TRequest : GetNotesByEntityRequestBase
    where TQuery : IQuery<IReadOnlyList<TDto>>
    where TDto : NoteDtoBase
{
    public override string Route => NotesConstants.DefaultNotesRoutePrefix;

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("NotesGetNotesByEntity").WithTags("Notes");
}

/// <summary>Ветка треда — ответы на заметку.</summary>
public abstract class GetNoteRepliesEndpoint<TRequest, TQuery, TDto>
    : QueryCollectionEndpoint<TRequest, TQuery, TDto, TDto>
    where TRequest : GetNoteRepliesRequestBase
    where TQuery : IQuery<IReadOnlyList<TDto>>
    where TDto : NoteDtoBase
{
    public override string Route => $"{NotesConstants.DefaultNotesRoutePrefix}/{{id:guid}}/replies";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("NotesGetNoteReplies").WithTags("Notes");
}

/// <summary>Правка тела заметки и набора упоминаний.</summary>
public abstract class UpdateNoteEndpoint<TRequest, TCommand> : UpdateCommandEndpoint<TRequest, TCommand>
    where TRequest : UpdateNoteRequestBase
    where TCommand : ICommand
{
    public override string Route => $"{NotesConstants.DefaultNotesRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("NotesUpdateNote").WithTags("Notes");
}

/// <summary>Закрепить заметку на карточке сущности.</summary>
public abstract class PinNoteEndpoint<TRequest, TCommand> : CommandEndpoint<TRequest, TCommand>
    where TRequest : NoteLifecycleRequestBase
    where TCommand : ICommand
{
    public override string Route => $"{NotesConstants.DefaultNotesRoutePrefix}/{{id:guid}}/pin";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("NotesPinNote").WithTags("Notes");
}

/// <summary>Открепить заметку.</summary>
public abstract class UnpinNoteEndpoint<TRequest, TCommand> : CommandEndpoint<TRequest, TCommand>
    where TRequest : NoteLifecycleRequestBase
    where TCommand : ICommand
{
    public override string Route => $"{NotesConstants.DefaultNotesRoutePrefix}/{{id:guid}}/unpin";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("NotesUnpinNote").WithTags("Notes");
}

/// <summary>Удалить заметку (soft-delete).</summary>
public abstract class RemoveNoteEndpoint<TRequest, TCommand> : DeleteCommandEndpoint<TRequest, TCommand>
    where TRequest : NoteLifecycleRequestBase
    where TCommand : ICommand
{
    public override string Route => $"{NotesConstants.DefaultNotesRoutePrefix}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("NotesRemoveNote").WithTags("Notes");
}
