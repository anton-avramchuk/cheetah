using Cheetah.Modules.Notes.Application.Abstractions;
using Cheetah.Modules.Notes.Contracts;
using Cheetah.Modules.Notes.Domain.Entities;
using Cheetah.Modules.Notes.Shared;

namespace Cheetah.Modules.Notes.Application.Tests;

/// <summary>Конкретная заметка наследника с доп. полем — для проверки generic-хендлеров.</summary>
public sealed class TestNote : NoteBase
{
    public string? Visibility { get; private set; }

    private TestNote() { }

    public static TestNote Create(TestCreateRequest r)
    {
        var n = new TestNote();
        n.InitializeCore(Guid.NewGuid(), r.EntityType, r.EntityId, r.AuthorId, r.Body,
            r.Mentions, r.AttachmentFileIds, r.ParentNoteId);
        n.Visibility = r.Visibility;
        return n;
    }
}

public sealed record TestCreateRequest : CreateNoteRequestBase
{
    public string? Visibility { get; init; }
}

public sealed record TestUpdateRequest : UpdateNoteRequestBase;

public sealed record TestNoteDto : NoteDtoBase
{
    public string? Visibility { get; init; }
}

public sealed class TestNoteFactory : INoteFactory<TestNote, TestCreateRequest>
{
    public TestNote Create(TestCreateRequest request) => TestNote.Create(request);
}

public sealed class TestNoteProjector : INoteProjector<TestNote, TestNoteDto>
{
    public TestNoteDto ToDto(TestNote n) => new()
    {
        Id = n.Id,
        EntityType = n.EntityType,
        EntityId = n.EntityId,
        AuthorId = n.AuthorId,
        Body = n.Body,
        Mentions = n.Mentions.ToArray(),
        AttachmentFileIds = n.AttachmentFileIds.ToArray(),
        ParentNoteId = n.ParentNoteId,
        PinnedAt = n.PinnedAt,
        CreatedAt = n.CreatedAt,
        UpdatedAt = n.UpdatedAt,
        Visibility = n.Visibility
    };
}

internal static class TestData
{
    public static TestCreateRequest CreateRequest(string body = "First comment") => new()
    {
        EntityType = EntityRefKeys.Deal,
        EntityId = Guid.NewGuid(),
        AuthorId = Guid.NewGuid(),
        Body = body
    };

    public static TestNote NewNote() => TestNote.Create(CreateRequest());
}
