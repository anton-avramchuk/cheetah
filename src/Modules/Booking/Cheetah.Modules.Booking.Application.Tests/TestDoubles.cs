using Cheetah.Core.Events;
using Cheetah.DistributedLock;
using Cheetah.Modules.Booking.Application.Abstractions;
using Cheetah.Modules.Booking.Contracts;
using Cheetah.Modules.Booking.Domain.Entities;
using Cheetah.Modules.Booking.Shared;

namespace Cheetah.Modules.Booking.Application.Tests;

// Сущности модуля абстрактны — тесты закрывают их минимальными sealed-наследниками.

internal sealed class TestBookingType : BookingTypeBase
{
    public static TestBookingType New(int durationMinutes = 60, Guid? host = null, string slug = "intro")
    {
        var t = new TestBookingType();
        t.InitializeCore(Guid.NewGuid(), host ?? Guid.NewGuid(), slug, "Intro call",
            durationMinutes, LocationKind.Video, maxAdvanceDays: 30, minNoticeMinutes: 0);
        return t;
    }
}

internal sealed class TestSchedule : AvailabilityScheduleBase
{
    public static TestSchedule New(string tz = "UTC", Guid? host = null)
    {
        var s = new TestSchedule();
        s.InitializeCore(Guid.NewGuid(), host ?? Guid.NewGuid(), tz);
        return s;
    }
}

internal sealed class TestBooking : BookingBase
{
    public static TestBooking Reserve(BookingTypeBase type, DateTimeOffset startUtc,
        IEnumerable<BookingAnswer>? answers = null)
    {
        var b = new TestBooking();
        b.InitializeCore(Guid.NewGuid(), type, startUtc, "Jane", "jane@example.com", "UTC", null,
            answers ?? Array.Empty<BookingAnswer>());
        return b;
    }
}

/// <summary>Реальная фабрика брони для тестов (конструирует TestBooking из запроса).</summary>
internal sealed class TestBookingFactory : IBookingFactory<TestBooking, TestBookingType>
{
    public TestBooking Create(TestBookingType type, DateTimeOffset startUtc, CreatePublicBookingRequest request)
        => TestBooking.Reserve(type, startUtc, request.Answers.Select(a => BookingAnswer.Create(a.Question, a.Value)));
}

/// <summary>Лок-хэндл-заглушка.</summary>
internal sealed class FakeLock : IDistributedLock
{
    public string Key { get; init; } = "k";
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

/// <summary>Шина событий, записывающая опубликованное (для проверок в тестах).</summary>
internal sealed class FakeEventBus : IEventBus
{
    public List<object> Published { get; } = new();

    public ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        Published.Add(@event!);
        return ValueTask.CompletedTask;
    }

    public ValueTask PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        Published.AddRange(events.Cast<object>());
        return ValueTask.CompletedTask;
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
    {
    }
}
