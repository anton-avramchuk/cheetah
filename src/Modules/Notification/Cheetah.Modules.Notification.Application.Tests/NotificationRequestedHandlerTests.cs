using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Email.DomainEvents;
using Cheetah.Modules.Notification.Application.EventHandlers;
using Cheetah.Modules.Notification.Domain.Abstractions;
using Cheetah.Modules.Notification.Domain.Entities;
using Cheetah.Modules.Notification.DomainEvents;
using Cheetah.Modules.Notification.Shared;
using Microsoft.Extensions.Logging;
using Moq;

namespace Cheetah.Modules.Notification.Application.Tests;

public class NotificationRequestedHandlerTests
{
    private readonly Mock<IRepository<NotificationMessage, Guid>> _notifications = new();
    private readonly Mock<IRepository<NotificationDispatch, Guid>> _dispatches = new();
    private readonly Mock<IRepository<RecipientContact, Guid>> _contacts = new();
    private readonly Mock<ITemplateRenderer> _renderer = new();
    private readonly Mock<IChannelRouter> _router = new();
    private readonly Mock<IEventBus> _eventBus = new();
    private readonly NotificationRequestedHandler _handler;

    public NotificationRequestedHandlerTests()
    {
        _handler = new NotificationRequestedHandler(
            _notifications.Object, _dispatches.Object, _contacts.Object,
            _renderer.Object, _router.Object, _eventBus.Object,
            Mock.Of<ILogger<NotificationRequestedHandler>>());
    }

    private static NotificationRequested Event(Guid id) => new(
        id, Guid.NewGuid(), "order.shipped", nameof(NotificationCategory.Transactional),
        new Dictionary<string, string> { ["order_number"] = "A-100" });

    [Fact]
    public async Task DuplicateNotification_IsSkipped_NoWork()
    {
        var id = Guid.NewGuid();
        var existing = NotificationMessage.Create(id, Guid.NewGuid(), "k", "Transactional", null);
        _notifications.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        await _handler.HandleAsync(Event(id));

        _notifications.Verify(r => r.Add(It.IsAny<NotificationMessage>()), Times.Never);
        _dispatches.Verify(r => r.Add(It.IsAny<NotificationDispatch>()), Times.Never);
        _eventBus.Verify(b => b.PublishAsync(It.IsAny<EmailRequested>(), It.IsAny<CancellationToken>()), Times.Never);
        _notifications.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EmailChannel_PublishesEmailRequested_BeforeSave()
    {
        var id = Guid.NewGuid();
        var evt = Event(id);
        _notifications.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((NotificationMessage?)null);
        _contacts.Setup(r => r.GetByIdAsync(evt.RecipientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(RecipientContact.Create(evt.RecipientUserId, "user@example.com"));
        _router.Setup(r => r.Resolve(It.IsAny<NotificationCategory>(), It.IsAny<RecipientContact?>(), It.IsAny<NotificationChannel?>()))
            .Returns(new[] { NotificationChannel.Email });
        _renderer.Setup(r => r.HasTemplate("order.shipped", NotificationChannel.Email)).Returns(true);
        _renderer.Setup(r => r.Render("order.shipped", NotificationChannel.Email, It.IsAny<IReadOnlyDictionary<string, string>>()))
            .Returns(new RenderedMessage("subj", "body"));

        await _handler.HandleAsync(evt);

        _notifications.Verify(r => r.Add(It.IsAny<NotificationMessage>()), Times.Once);
        _dispatches.Verify(r => r.Add(It.IsAny<NotificationDispatch>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<EmailRequested>(e => e.ToAddress == "user@example.com" && e.NotificationId == id),
            It.IsAny<CancellationToken>()), Times.Once);
        _notifications.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NoDeliverableChannel_SavesMessage_WithoutPublishing()
    {
        var id = Guid.NewGuid();
        var evt = Event(id);
        _notifications.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((NotificationMessage?)null);
        _contacts.Setup(r => r.GetByIdAsync(evt.RecipientUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RecipientContact?)null);
        _router.Setup(r => r.Resolve(It.IsAny<NotificationCategory>(), It.IsAny<RecipientContact?>(), It.IsAny<NotificationChannel?>()))
            .Returns(Array.Empty<NotificationChannel>());

        await _handler.HandleAsync(evt);

        _eventBus.Verify(b => b.PublishAsync(It.IsAny<EmailRequested>(), It.IsAny<CancellationToken>()), Times.Never);
        _notifications.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
