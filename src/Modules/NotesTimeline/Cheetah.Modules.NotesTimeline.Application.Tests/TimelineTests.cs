using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Core.Specification;
using Cheetah.Modules.NotesTimeline.Application.Timeline;
using Cheetah.Modules.NotesTimeline.Domain.Abstractions;
using Cheetah.Modules.NotesTimeline.Domain.Entities;
using Cheetah.Modules.NotesTimeline.Infrastructure.Timeline;
using Cheetah.Modules.NotesTimeline.Shared;
using Moq;
using Shouldly;

namespace Cheetah.Modules.NotesTimeline.Application.Tests;

public record TestTimelineEvent(Guid EntityId, bool Significant) : EventBase;

public sealed class TestTimelineProjector : ITimelineProjector<TestTimelineEvent>
{
    public string Kind => TimelineKinds.DealStageChanged;
    public bool CanProject(TestTimelineEvent @event) => @event.Significant;
    public TimelineEntry Project(TestTimelineEvent @event)
        => TimelineEntry.Create(EntityRefKeys.Deal, @event.EntityId, Kind, "Факт", DateTimeOffset.UtcNow,
            sourceEventId: @event.EventId.ToString());
}

public class GetTimelineQueryHandlerTests
{
    private readonly Mock<IRepository<TimelineEntry, Guid>> _repo = new();

    [Fact]
    public async Task Get_OrdersDescByOccurred_AppliesLimit_AndReturnsCursor()
    {
        var entityId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var entries = Enumerable.Range(0, 5)
            .Select(i => TimelineEntry.Create(EntityRefKeys.Deal, entityId, TimelineKinds.NoteAdded,
                $"e{i}", now.AddMinutes(-i)))
            .ToList();

        _repo.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<TimelineEntry>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var handler = new GetTimelineQueryHandler(_repo.Object);
        var page = await handler.HandleAsync(new GetTimelineQuery(EntityRefKeys.Deal, entityId, null, null, Limit: 3));

        page.Items.Count.ShouldBe(3);
        page.Items[0].Title.ShouldBe("e0"); // самый свежий
        page.Items[2].Title.ShouldBe("e2");
        page.NextCursor.ShouldBe(entries[2].OccurredAt); // полная страница → есть курсор
    }

    [Fact]
    public async Task Get_ParsesJsonPayload()
    {
        var entry = TimelineEntry.Create(EntityRefKeys.Deal, Guid.NewGuid(), TimelineKinds.NoteAdded,
            "t", DateTimeOffset.UtcNow, payload: "{\"amount\":42}");
        _repo.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<TimelineEntry>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TimelineEntry> { entry });

        var handler = new GetTimelineQueryHandler(_repo.Object);
        var page = await handler.HandleAsync(new GetTimelineQuery(EntityRefKeys.Deal, entry.EntityId, null, null, null));

        page.Items[0].Payload.ShouldNotBeNull();
        page.Items[0].Payload!.Value.GetProperty("amount").GetInt32().ShouldBe(42);
        page.NextCursor.ShouldBeNull(); // неполная страница → конец
    }
}

public class TimelineProjectionServiceTests
{
    [Fact]
    public async Task Project_WritesEntry_WhenProjectorCanProject()
    {
        var writer = new Mock<ITimelineWriter>();
        var service = new TimelineProjectionService<TestTimelineEvent>(
            new[] { new TestTimelineProjector() }, writer.Object);

        await service.ProjectAsync(new TestTimelineEvent(Guid.NewGuid(), Significant: true));

        writer.Verify(w => w.AppendAsync(It.IsAny<TimelineEntry>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Project_SkipsEntry_WhenProjectorRejects()
    {
        var writer = new Mock<ITimelineWriter>();
        var service = new TimelineProjectionService<TestTimelineEvent>(
            new[] { new TestTimelineProjector() }, writer.Object);

        await service.ProjectAsync(new TestTimelineEvent(Guid.NewGuid(), Significant: false));

        writer.Verify(w => w.AppendAsync(It.IsAny<TimelineEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class TimelineWriterTests
{
    private readonly Mock<IRepository<TimelineEntry, Guid>> _repo = new();

    [Fact]
    public async Task Append_NewEntry_AddsAndSaves()
    {
        _repo.Setup(r => r.ExistsAsync(It.IsAny<ISpecification<TimelineEntry>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var writer = new TimelineWriter(_repo.Object);
        var entry = TimelineEntry.Create(EntityRefKeys.Deal, Guid.NewGuid(), TimelineKinds.NoteAdded,
            "t", DateTimeOffset.UtcNow, sourceEventId: "evt-1");

        var appended = await writer.AppendAsync(entry);

        appended.ShouldBeTrue();
        _repo.Verify(r => r.Add(entry), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Append_DuplicateSource_SkipsWrite()
    {
        _repo.Setup(r => r.ExistsAsync(It.IsAny<ISpecification<TimelineEntry>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var writer = new TimelineWriter(_repo.Object);
        var entry = TimelineEntry.Create(EntityRefKeys.Deal, Guid.NewGuid(), TimelineKinds.NoteAdded,
            "t", DateTimeOffset.UtcNow, sourceEventId: "evt-dup");

        var appended = await writer.AppendAsync(entry);

        appended.ShouldBeFalse();
        _repo.Verify(r => r.Add(It.IsAny<TimelineEntry>()), Times.Never);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Append_NullSource_AlwaysWrites()
    {
        var writer = new TimelineWriter(_repo.Object);
        var entry = TimelineEntry.Create(EntityRefKeys.Deal, Guid.NewGuid(), TimelineKinds.NoteAdded,
            "t", DateTimeOffset.UtcNow);

        var appended = await writer.AppendAsync(entry);

        appended.ShouldBeTrue();
        _repo.Verify(r => r.ExistsAsync(It.IsAny<ISpecification<TimelineEntry>>(), It.IsAny<CancellationToken>()), Times.Never);
        _repo.Verify(r => r.Add(entry), Times.Once);
    }
}
