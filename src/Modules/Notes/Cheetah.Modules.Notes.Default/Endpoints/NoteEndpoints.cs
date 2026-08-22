using Cheetah.Modules.Notes.Api.Endpoints;
using Cheetah.Modules.Notes.Application.Notes;
using Cheetah.Modules.Notes.Default.Contracts;

namespace Cheetah.Modules.Notes.Default.Endpoints;

/// <summary>
/// Конкретные эндпоинты «из коробки»: закрывают абстрактные шаблоны Api-слоя типами Default.
/// Генератор <c>Cheetah.Generators.Endpoints</c> регистрирует их в
/// <c>CheetahNotesDefaultModule.OnApplicationInitialization</c>.
/// </summary>
public sealed class CreateNoteEndpoint
    : CreateNoteEndpoint<CreateNoteRequest, CreateNoteCommand<CreateNoteRequest>>;

public sealed class GetNoteByIdEndpoint
    : GetNoteByIdEndpoint<GetNoteByIdRequest, GetNoteByIdQuery<NoteDto>, NoteDto>;

public sealed class GetNotesByEntityEndpoint
    : GetNotesByEntityEndpoint<GetNotesByEntityRequest, GetNotesByEntityQuery<NoteDto>, NoteDto>;

public sealed class GetNoteRepliesEndpoint
    : GetNoteRepliesEndpoint<GetNoteRepliesRequest, GetNoteRepliesQuery<NoteDto>, NoteDto>;

public sealed class UpdateNoteEndpoint
    : UpdateNoteEndpoint<UpdateNoteRequest, UpdateNoteCommand<UpdateNoteRequest>>;

public sealed class PinNoteEndpoint : PinNoteEndpoint<PinNoteRequest, PinNoteCommand>;

public sealed class UnpinNoteEndpoint : UnpinNoteEndpoint<UnpinNoteRequest, UnpinNoteCommand>;

public sealed class RemoveNoteEndpoint : RemoveNoteEndpoint<RemoveNoteRequest, RemoveNoteCommand>;
