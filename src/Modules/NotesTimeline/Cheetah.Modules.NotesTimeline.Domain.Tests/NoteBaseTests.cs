using Cheetah.Modules.NotesTimeline.DomainEvents;
using Cheetah.Modules.NotesTimeline.Shared;
using Shouldly;

namespace Cheetah.Modules.NotesTimeline.Domain.Tests;

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
    public void Create_BlankBody_Throws() =>
        Should.Throw<ArgumentException>(() =>
            TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "  "));

    [Fact]
    public void Edit_RaisesUpdated_AndOnlyNewMentionEvents()
    {
        var existing = Guid.NewGuid();
        var note = TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "body",
            mentions: new[] { existing });
        note.ClearDomainEvents();

        var added = Guid.NewGuid();
        note.Edit("updated", mentions: new[] { existing, added });

        note.Body.ShouldBe("updated");
        note.Mentions.ShouldBe(new[] { existing, added });
        note.DomainEvents.OfType<NoteUpdatedIntegrationEvent>().Count().ShouldBe(1);
        var mentionEvents = note.DomainEvents.OfType<UserMentionedIntegrationEvent>().ToArray();
        mentionEvents.Length.ShouldBe(1);
        mentionEvents[0].MentionedUserId.ShouldBe(added); // не повторяем уже упомянутого
    }

    [Fact]
    public void Pin_SetsPinnedAt_Unpin_Clears()
    {
        var note = TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "x");

        note.Pin();
        note.PinnedAt.ShouldNotBeNull();

        note.Unpin();
        note.PinnedAt.ShouldBeNull();
    }

    [Fact]
    public void Remove_SetsRemovedAt_AndRaisesRemovedEventOnce()
    {
        var note = TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "x");
        note.ClearDomainEvents();

        note.Remove();
        note.Remove(); // повторный вызов — без эффекта

        note.RemovedAt.ShouldNotBeNull();
        note.DomainEvents.OfType<NoteRemovedIntegrationEvent>().Count().ShouldBe(1);
    }

    [Fact]
    public void ExtensionField_IsIndependentOfBase()
    {
        var note = TestNote.Create(EntityRefKeys.Deal, Guid.NewGuid(), Guid.NewGuid(), "x");
        note.SetVisibility("private");
        note.Visibility.ShouldBe("private");
    }
}
