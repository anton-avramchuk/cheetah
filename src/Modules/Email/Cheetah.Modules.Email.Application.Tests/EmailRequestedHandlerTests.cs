using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Email.Application.EventHandlers;
using Cheetah.Modules.Email.Domain.Abstractions;
using Cheetah.Modules.Email.Domain.Entities;
using Cheetah.Modules.Email.DomainEvents;
using Microsoft.Extensions.Logging;
using Moq;

namespace Cheetah.Modules.Email.Application.Tests;

public class EmailRequestedHandlerTests
{
    private readonly Mock<IRepository<SentEmail, Guid>> _sent = new();
    private readonly Mock<IEmailGateway> _gateway = new();
    private readonly Mock<IEventBus> _eventBus = new();
    private readonly EmailRequestedHandler _handler;

    public EmailRequestedHandlerTests()
    {
        _handler = new EmailRequestedHandler(
            _sent.Object, _gateway.Object, _eventBus.Object,
            Mock.Of<ILogger<EmailRequestedHandler>>());
    }

    private static EmailRequested Event(Guid dispatchId) => new(
        dispatchId, Guid.NewGuid(), "user@example.com", "subj", "<p>body</p>", null, "Transactional");

    private void SetupGateway(EmailSendResult result) => _gateway
        .Setup(g => g.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(result);

    [Fact]
    public async Task DuplicateDispatch_IsSkipped_GatewayNotCalled()
    {
        var dispatchId = Guid.NewGuid();
        _sent.Setup(r => r.GetByIdAsync(dispatchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(SentEmail.Create(dispatchId, Guid.NewGuid(), "user@example.com"));

        await _handler.HandleAsync(Event(dispatchId));

        _gateway.Verify(g => g.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _sent.Verify(r => r.Add(It.IsAny<SentEmail>()), Times.Never);
        _eventBus.Verify(b => b.PublishAsync(It.IsAny<EmailDelivered>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GatewaySuccess_PublishesEmailDelivered_AndSaves()
    {
        var dispatchId = Guid.NewGuid();
        _sent.Setup(r => r.GetByIdAsync(dispatchId, It.IsAny<CancellationToken>())).ReturnsAsync((SentEmail?)null);
        SetupGateway(EmailSendResult.Ok("provider-msg-1"));

        await _handler.HandleAsync(Event(dispatchId));

        _sent.Verify(r => r.Add(It.IsAny<SentEmail>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<EmailDelivered>(e => e.DispatchId == dispatchId && e.ProviderMessageId == "provider-msg-1"),
            It.IsAny<CancellationToken>()), Times.Once);
        _sent.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GatewayFailure_PublishesEmailFailed()
    {
        var dispatchId = Guid.NewGuid();
        _sent.Setup(r => r.GetByIdAsync(dispatchId, It.IsAny<CancellationToken>())).ReturnsAsync((SentEmail?)null);
        SetupGateway(EmailSendResult.Fail(EmailFailureReason.InvalidAddress, isPermanent: true, "bad address"));

        await _handler.HandleAsync(Event(dispatchId));

        _eventBus.Verify(b => b.PublishAsync(
            It.Is<EmailFailed>(e => e.DispatchId == dispatchId && e.Reason == EmailFailureReason.InvalidAddress && e.IsPermanent),
            It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(It.IsAny<EmailDelivered>(), It.IsAny<CancellationToken>()), Times.Never);
        _sent.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
