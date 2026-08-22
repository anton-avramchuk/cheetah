using Cheetah.Modules.Notes.DomainEvents;
using Cheetah.Modules.Notes.Shared;
using Shouldly;

namespace Cheetah.Modules.Notes.Domain.Tests;

public class NoteBaseTests
{
    [Fact]
    public void Create_RaisesCreatedEvent_AndOneMentionEventPerMention()
    {
        var m1 = Guid.NewGuid();
        var m2 = Guid.NewGuid();

        var note = TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "  hello  ",
            mentions: new[] { m1, m2 });

        note.Body.ShouldBe("hello"); // trimmed
        note.Mentions.ShouldBe(new[] { m1, m2 });
        note.DomainEvents.OfType<NoteCreatedIntegrationEvent>().Count().ShouldBe(1);
        note.DomainEvents.OfType<UserMentionedIntegrationEvent>().Count().ShouldBe(2);
    }

    [Fact]
    public void Create_DeduplicatesMentions()
    {
        var m = Guid.NewGuid();
        var note = TestNote.Create(EntityRefKeys.Customer, Guid.NewGuid(), Guid.NewGuid(), "x",
            mentions: new[] { m, m });

        note.Mentions.Count.ShouldBe(1);
        note.DomainEvents.OfType<UserMentionedIntegrationEvent>().Count().ShouldBe(1);
    }

    [Fact]
    public void Create_AnyEntityType_IsAccepted()
    {
        var note = TestNote.Create("app.custom-entity", Guid.NewGuid(), Guid.NewGuid(), "x");

        note.EntityType.ShouldBe("app.custom-entity");
    }

    [Fact]
    public void Create_BlankBody_Throws() =>
        Should.Throw<ArgumentException>(() =>
            TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "  "));

    [Fact]
    public void Create_BlankEntityType_Throws() =>
        Should.Throw<ArgumentException>(() =>
            TestNote.Create("  ", Guid.NewGuid(), Guid.NewGuid(), "body"));

    [Fact]
    public void Create_Reply_KeepsParentLink()
    {
        var parentId = Guid.NewGuid();

        var reply = TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "re:",
            parentNoteId: parentId);

        reply.ParentNoteId.ShouldBe(parentId);
    }

    [Fact]
    public void Edit_PublishesMentionEventOnlyForNewMentions()
    {
        var known = Guid.NewGuid();
        var added = Guid.NewGuid();
        var note = TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "body",
            mentions: new[] { known });
        note.ClearDomainEvents();

        note.Edit("updated", new[] { known, added });

        note.Body.ShouldBe("updated");
        note.DomainEvents.OfType<NoteUpdatedIntegrationEvent>().Count().ShouldBe(1);
        var mentionEvents = note.DomainEvents.OfType<UserMentionedIntegrationEvent>().ToArray();
        mentionEvents.Length.ShouldBe(1);
        mentionEvents[0].MentionedUserId.ShouldBe(added);
    }

    [Fact]
    public void Edit_BlankBody_Throws()
    {
        var note = TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "body");

        Should.Throw<ArgumentException>(() => note.Edit(" "));
    }

    [Fact]
    public void Edit_WithoutMentions_KeepsExistingOnes()
    {
        var mention = Guid.NewGuid();
        var note = TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "body",
            mentions: new[] { mention });
        note.ClearDomainEvents();

        note.Edit("just a typo fix");

        note.Mentions.ShouldBe(new[] { mention });
        note.DomainEvents.OfType<UserMentionedIntegrationEvent>().ShouldBeEmpty();
    }

    [Fact]
    public void Edit_WithEmptyMentions_ClearsThem()
    {
        var note = TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "body",
            mentions: new[] { Guid.NewGuid() });

        note.Edit("body", Array.Empty<Guid>());

        note.Mentions.ShouldBeEmpty();
    }

    [Fact]
    public void Create_TooLongBody_Throws() =>
        Should.Throw<ArgumentException>(() => TestNote.Create(
            EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(),
            new string('x', NotesConstants.MaxBodyLength + 1)));

    [Fact]
    public void Create_TooLongEntityType_Throws() =>
        Should.Throw<ArgumentException>(() => TestNote.Create(
            new string('x', NotesConstants.MaxEntityTypeLength + 1), Guid.NewGuid(), Guid.NewGuid(), "body"));

    [Fact]
    public void Edit_TooLongBody_Throws()
    {
        var note = TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "body");

        Should.Throw<ArgumentException>(() => note.Edit(new string('x', NotesConstants.MaxBodyLength + 1)));
    }

    [Fact]
    public void ChangeAttachments_ReplacesSet_AndNullKeepsIt()
    {
        var initial = Guid.NewGuid();
        var note = TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "body",
            attachmentFileIds: new[] { initial });

        note.ChangeAttachments(null);
        note.AttachmentFileIds.ShouldBe(new[] { initial });

        var replacement = Guid.NewGuid();
        note.ChangeAttachments(new[] { replacement });
        note.AttachmentFileIds.ShouldBe(new[] { replacement });

        note.ChangeAttachments(Array.Empty<Guid>());
        note.AttachmentFileIds.ShouldBeEmpty();
    }

    [Fact]
    public void Pin_IsIdempotent_AndRaisesEventOnce()
    {
        var note = TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "body");
        note.ClearDomainEvents();

        note.Pin();
        var first = note.PinnedAt;
        note.Pin();

        note.PinnedAt.ShouldBe(first);
        note.DomainEvents.OfType<NotePinnedIntegrationEvent>().Count().ShouldBe(1);
    }

    [Fact]
    public void Unpin_ClearsPin_AndRaisesEventOnlyWhenPinned()
    {
        var note = TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "body");
        note.Pin();
        note.ClearDomainEvents();

        note.Unpin();
        note.Unpin();

        note.PinnedAt.ShouldBeNull();
        note.DomainEvents.OfType<NoteUnpinnedIntegrationEvent>().Count().ShouldBe(1);
    }

    [Fact]
    public void Remove_SoftDeletes_AndPublishesEventOnce()
    {
        var note = TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "body");
        note.ClearDomainEvents();

        note.Remove();
        note.Remove();

        note.RemovedAt.ShouldNotBeNull();
        note.DomainEvents.OfType<NoteRemovedIntegrationEvent>().Count().ShouldBe(1);
    }

    [Fact]
    public void SetAttributes_StoresSchemalessPayload()
    {
        var note = TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "body");

        note.SetAttributes("""{"source":"email"}""");

        note.Attributes.ShouldBe("""{"source":"email"}""");
    }

    [Fact]
    public void Extension_AdditionalFieldLivesOnHeir()
    {
        var note = TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "body");

        note.SetVisibility("team");

        note.Visibility.ShouldBe("team");
    }
}
