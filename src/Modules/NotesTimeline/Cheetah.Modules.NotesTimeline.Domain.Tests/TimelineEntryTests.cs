using Cheetah.Modules.NotesTimeline.Domain.Entities;
using Cheetah.Modules.NotesTimeline.Shared;
using Shouldly;

namespace Cheetah.Modules.NotesTimeline.Domain.Tests;

public class TimelineEntryTests
{
    [Fact]
    public void Create_SetsAllFields()
    {
        var entityId = Guid.NewGuid();
        var actor = Guid.NewGuid();
        var occurred = DateTimeOffset.UtcNow;

        var entry = TimelineEntry.Create(EntityRefKeys.Deal, entityId, TimelineKinds.DealStageChanged,
            "  Сделка сменила стадию  ", occurred, actor, payload: "{\"x\":1}", sourceEventId: "evt-1");

        entry.Id.ShouldNotBe(Guid.Empty);
        entry.EntityType.ShouldBe(EntityRefKeys.Deal);
        entry.EntityId.ShouldBe(entityId);
        entry.Kind.ShouldBe(TimelineKinds.DealStageChanged);
        entry.Title.ShouldBe("Сделка сменила стадию"); // trimmed
        entry.OccurredAt.ShouldBe(occurred);
        entry.ActorId.ShouldBe(actor);
        entry.Payload.ShouldBe("{\"x\":1}");
        entry.SourceEventId.ShouldBe("evt-1");
    }

    [Theory]
    [InlineData("", "kind", "title")]
    [InlineData("crm.deal", "", "title")]
    [InlineData("crm.deal", "kind", "")]
    public void Create_BlankRequiredField_Throws(string entityType, string kind, string title) =>
        Should.Throw<ArgumentException>(() =>
            TimelineEntry.Create(entityType, Guid.NewGuid(), kind, title, DateTimeOffset.UtcNow));
}
