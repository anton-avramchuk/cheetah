using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Specification;
using Cheetah.DistributedLock;
using Cheetah.Modules.Booking.Application.Abstractions;
using Cheetah.Modules.Booking.Application.Bookings;
using Cheetah.Modules.Booking.Application.Exceptions;
using Cheetah.Modules.Booking.Contracts;
using Cheetah.Modules.Booking.Domain.Entities;
using Cheetah.Modules.Booking.Domain.Slots;
using Cheetah.Modules.Booking.DomainEvents;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Booking.Application.Tests;

public class CreateBookingCommandHandlerTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 5, 9, 0, 0, TimeSpan.Zero);

    private sealed class Harness
    {
        public TestBookingType Type { get; } = TestBookingType.New(durationMinutes: 60);
        public Mock<IRepository<TestBooking, Guid>> Bookings { get; } = new();
        public Mock<IRepository<TestBookingType, Guid>> Types { get; } = new();
        public Mock<IBookingCalendarGateway> Calendar { get; } = new();
        public Mock<IDistributedLockProvider> Locks { get; } = new();
        public FakeEventBus EventBus { get; } = new();

        public Harness()
        {
            Types.Setup(r => r.GetBySpecAsync(It.IsAny<ISpecification<TestBookingType>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Type);
            Locks.Setup(l => l.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FakeLock());
            Calendar.Setup(c => c.GetBusyAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<BusyInterval>());
            Calendar.Setup(c => c.CreateEventAsync(It.IsAny<BookingBase>(), It.IsAny<BookingTypeBase>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid());
            Bookings.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<TestBooking>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TestBooking>());
            Bookings.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        }

        public CreateBookingCommandHandler<TestBooking, TestBookingType> Build() => new(
            Bookings.Object, Types.Object, new TestBookingFactory(), Calendar.Object,
            Locks.Object, new NullBookingConfirmationHandler<TestBooking>(), EventBus);

        public static CreateBookingCommand Command() =>
            new("intro", new CreatePublicBookingRequest(Start, "Jane", "jane@example.com", null, "UTC",
                Array.Empty<BookingAnswerDto>()));
    }

    [Fact]
    public async Task HappyPath_CreatesBooking_AttachesCalendarEvent_PublishesConfirmed()
    {
        var h = new Harness();
        var id = await h.Build().HandleAsync(Harness.Command());

        id.ShouldNotBe(Guid.Empty);
        h.Bookings.Verify(r => r.Add(It.IsAny<TestBooking>()), Times.Once);
        h.Calendar.Verify(c => c.CreateEventAsync(It.IsAny<BookingBase>(), It.IsAny<BookingTypeBase>(), It.IsAny<CancellationToken>()), Times.Once);
        h.EventBus.Published.OfType<BookingConfirmedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public async Task UnknownSlug_Throws()
    {
        var h = new Harness();
        h.Types.Setup(r => r.GetBySpecAsync(It.IsAny<ISpecification<TestBookingType>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestBookingType?)null);

        await Should.ThrowAsync<BookingValidationException>(() => h.Build().HandleAsync(Harness.Command()).AsTask());
    }

    [Fact]
    public async Task LockNotAcquired_ThrowsConflict()
    {
        var h = new Harness();
        h.Locks.Setup(l => l.TryAcquireAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IDistributedLock?)null);

        await Should.ThrowAsync<BookingConflictException>(() => h.Build().HandleAsync(Harness.Command()).AsTask());
    }

    [Fact]
    public async Task SlotBusyInCalendar_ThrowsConflict()
    {
        var h = new Harness();
        h.Calendar.Setup(c => c.GetBusyAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new BusyInterval(Start.AddMinutes(15), Start.AddMinutes(45)) });

        await Should.ThrowAsync<BookingConflictException>(() => h.Build().HandleAsync(Harness.Command()).AsTask());
        h.Bookings.Verify(r => r.Add(It.IsAny<TestBooking>()), Times.Never);
    }

    [Fact]
    public async Task ExistingActiveBooking_ThrowsConflict()
    {
        var h = new Harness();
        h.Bookings.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<TestBooking>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestBooking> { TestBooking.Reserve(h.Type, Start) });

        await Should.ThrowAsync<BookingConflictException>(() => h.Build().HandleAsync(Harness.Command()).AsTask());
    }
}
