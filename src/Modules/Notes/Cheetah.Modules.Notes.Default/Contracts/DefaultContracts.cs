using Cheetah.Modules.Notes.Contracts;

namespace Cheetah.Modules.Notes.Default.Contracts;

/// <summary>Конкретные Contracts «из коробки»: закрывают абстрактные базы без доп. полей.</summary>
public sealed record CreateNoteRequest : CreateNoteRequestBase;

public sealed record UpdateNoteRequest : UpdateNoteRequestBase;

public sealed record GetNoteByIdRequest : GetNoteByIdRequestBase;

public sealed record GetNotesByEntityRequest : GetNotesByEntityRequestBase;

public sealed record GetNoteRepliesRequest : GetNoteRepliesRequestBase;

public sealed record PinNoteRequest : NoteLifecycleRequestBase;

public sealed record UnpinNoteRequest : NoteLifecycleRequestBase;

public sealed record RemoveNoteRequest : NoteLifecycleRequestBase;

public sealed record NoteDto : NoteDtoBase;
