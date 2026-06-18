using Cheetah.Core.Specification;
using Cheetah.Modules.Calendar.Application.Events;
using Cheetah.Modules.Calendar.Domain.Abstractions;
using Cheetah.Modules.Calendar.Domain.Entities;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Calendar.Application.Tests;

public class GetUserBusyQueryTests
{
    private static readonly Guid Host = Guid.NewGuid();
    private static readonly DateTime From = new(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime To = new(2026, 1, 6, 0, 0, 0, DateTimeKind.Utc);

    private static CalendarEvent Event(DateTime startUtc, DateTime endUtc)
        => CalendarEvent.Schedule(Guid.NewGuid(), "M", startUtc, endUtc, Host);

    private static Occurrence Occ(DateTime startUtc, DateTime endUtc)
        => new(startUtc, endUtc, "k", false);

    private static (Mock<ICalendarEventRepository> repo, Mock<IRecurrenceExpander> exp) Mocks(
        params (CalendarEvent ev, Occurrence[] occ)[] events)
    {
        var repo = new Mock<ICalendarEventRepository>();
        repo.Setup(r => r.ListWithDetailsAsync(It.IsAny<ISpecification<CalendarEvent>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(events.Select(e => e.ev).ToList());

        var exp = new Mock<IRecurrenceExpander>();
        foreach (var (ev, occ) in events)
            exp.Setup(x => x.Expand(ev, It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(occ);

        return (repo, exp);
    }

    [Fact]
    public async Task SingleEvent_ProducesSingleInterval()
    {
        var ev = Event(From.AddHours(9), From.AddHours(9.5));
        var (repo, exp) = Mocks((ev, new[] { Occ(From.AddHours(9), From.AddHours(9.5)) }));
        var handler = new GetUserBusyQueryHandler(repo.Object, exp.Object);

        var result = await handler.HandleAsync(new GetUserBusyQuery(Host, From, To));

        result.ShouldHaveSingleItem();
        result[0].StartUtc.ShouldBe(From.AddHours(9));
        result[0].EndUtc.ShouldBe(From.AddHours(9.5));
    }

    [Fact]
    public async Task OverlappingOccurrences_AreMerged()
    {
        var a = Event(From.AddHours(10), From.AddHours(10.5));
        var b = Event(From.AddHours(10.25), From.AddHours(11));
        var (repo, exp) = Mocks(
            (a, new[] { Occ(From.AddHours(10), From.AddHours(10.5)) }),
            (b, new[] { Occ(From.AddHours(10.25), From.AddHours(11)) }));
        var handler = new GetUserBusyQueryHandler(repo.Object, exp.Object);

        var result = await handler.HandleAsync(new GetUserBusyQuery(Host, From, To));

        result.ShouldHaveSingleItem();
        result[0].StartUtc.ShouldBe(From.AddHours(10));
        result[0].EndUtc.ShouldBe(From.AddHours(11));
    }

    [Fact]
    public async Task AdjacentOccurrences_AreMerged()
    {
        var a = Event(From.AddHours(9), From.AddHours(10));
        var b = Event(From.AddHours(10), From.AddHours(11));
        var (repo, exp) = Mocks(
            (a, new[] { Occ(From.AddHours(9), From.AddHours(10)) }),
            (b, new[] { Occ(From.AddHours(10), From.AddHours(11)) }));
        var handler = new GetUserBusyQueryHandler(repo.Object, exp.Object);

        var result = await handler.HandleAsync(new GetUserBusyQuery(Host, From, To));

        result.ShouldHaveSingleItem();
        result[0].StartUtc.ShouldBe(From.AddHours(9));
        result[0].EndUtc.ShouldBe(From.AddHours(11));
    }

    [Fact]
    public async Task DisjointOccurrences_Stayseparate()
    {
        var a = Event(From.AddHours(9), From.AddHours(9.5));
        var b = Event(From.AddHours(14), From.AddHours(15));
        var (repo, exp) = Mocks(
            (a, new[] { Occ(From.AddHours(9), From.AddHours(9.5)) }),
            (b, new[] { Occ(From.AddHours(14), From.AddHours(15)) }));
        var handler = new GetUserBusyQueryHandler(repo.Object, exp.Object);

        var result = await handler.HandleAsync(new GetUserBusyQuery(Host, From, To));

        result.Count.ShouldBe(2);
        result[0].StartUtc.ShouldBe(From.AddHours(9));
        result[1].StartUtc.ShouldBe(From.AddHours(14));
    }

    [Fact]
    public async Task SeriesOccurrences_AreExpandedAndMerged()
    {
        // Одно событие-серия → несколько экземпляров; пересекающиеся сливаются, раздельные — нет.
        var ev = Event(From.AddHours(9), From.AddHours(10));
        var (repo, exp) = Mocks((ev, new[]
        {
            Occ(From.AddHours(9), From.AddHours(10)),
            Occ(From.AddHours(9.5), From.AddHours(11)),  // пересекается с первым → слить
            Occ(From.AddHours(13), From.AddHours(14)),   // отдельный
        }));
        var handler = new GetUserBusyQueryHandler(repo.Object, exp.Object);

        var result = await handler.HandleAsync(new GetUserBusyQuery(Host, From, To));

        result.Count.ShouldBe(2);
        result[0].StartUtc.ShouldBe(From.AddHours(9));
        result[0].EndUtc.ShouldBe(From.AddHours(11));
        result[1].StartUtc.ShouldBe(From.AddHours(13));
    }

    [Fact]
    public async Task OccurrencesOutsideWindow_AreClipped()
    {
        var ev = Event(From.AddHours(-1), From.AddHours(1));
        var (repo, exp) = Mocks((ev, new[]
        {
            Occ(From.AddHours(-1), From.AddHours(1)),  // начинается до окна → обрезать слева до From
            Occ(To.AddHours(-0.5), To.AddHours(2)),    // заканчивается после окна → обрезать справа до To
        }));
        var handler = new GetUserBusyQueryHandler(repo.Object, exp.Object);

        var result = await handler.HandleAsync(new GetUserBusyQuery(Host, From, To));

        result.Count.ShouldBe(2);
        result[0].StartUtc.ShouldBe(From);              // обрезано по нижней границе окна
        result[1].EndUtc.ShouldBe(To);                  // обрезано по верхней границе окна
    }

    [Fact]
    public async Task NoEvents_ReturnsEmpty()
    {
        var (repo, exp) = Mocks();
        var handler = new GetUserBusyQueryHandler(repo.Object, exp.Object);

        var result = await handler.HandleAsync(new GetUserBusyQuery(Host, From, To));

        result.ShouldBeEmpty();
    }
}
