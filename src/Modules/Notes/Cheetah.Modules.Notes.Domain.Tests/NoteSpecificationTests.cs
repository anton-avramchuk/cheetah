using Cheetah.Modules.Notes.Domain.Specifications;
using Cheetah.Modules.Notes.Shared;
using Shouldly;

namespace Cheetah.Modules.Notes.Domain.Tests;

public class NoteSpecificationTests
{
    private static readonly Guid EntityId = Guid.NewGuid();

    private static TestNote Note(string entityType = EntityRefKeys.Deal, Guid? entityId = null,
        Guid? authorId = null, Guid? parentId = null)
        => TestNote.Create(entityType, entityId ?? EntityId, authorId ?? Guid.NewGuid(), "body",
            parentNoteId: parentId);

    [Fact]
    public void ByEntity_MatchesOnlySameEntityRef()
    {
        var spec = new NotesByEntitySpecification<TestNote>(EntityRefKeys.Deal, EntityId).ToExpression().Compile();

        spec(Note()).ShouldBeTrue();
        spec(Note(entityType: EntityRefKeys.Customer)).ShouldBeFalse();
        spec(Note(entityId: Guid.NewGuid())).ShouldBeFalse();
    }

    [Fact]
    public void ByEntity_ExcludesRemoved()
    {
        var removed = Note();
        removed.Remove();

        var spec = new NotesByEntitySpecification<TestNote>(EntityRefKeys.Deal, EntityId).ToExpression().Compile();

        spec(removed).ShouldBeFalse();
    }

    [Fact]
    public void PinnedByEntity_MatchesOnlyPinned()
    {
        var pinned = Note();
        pinned.Pin();

        var spec = new PinnedNotesByEntitySpecification<TestNote>(EntityRefKeys.Deal, EntityId)
            .ToExpression().Compile();

        spec(pinned).ShouldBeTrue();
        spec(Note()).ShouldBeFalse();
    }

    [Fact]
    public void ByAuthor_MatchesOwnActiveNotes()
    {
        var author = Guid.NewGuid();
        var spec = new NotesByAuthorSpecification<TestNote>(author).ToExpression().Compile();

        spec(Note(authorId: author)).ShouldBeTrue();
        spec(Note()).ShouldBeFalse();
    }

    [Fact]
    public void Thread_MatchesRepliesOfParent()
    {
        var parent = Guid.NewGuid();
        var spec = new NoteThreadSpecification<TestNote>(parent).ToExpression().Compile();

        spec(Note(parentId: parent)).ShouldBeTrue();
        spec(Note()).ShouldBeFalse();
    }
}
