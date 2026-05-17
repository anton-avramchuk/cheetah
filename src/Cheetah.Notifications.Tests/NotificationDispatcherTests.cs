using Cheetah.Notifications;
using Cheetah.Notifications.Email;
using Cheetah.Notifications.Sms;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;

namespace Cheetah.Notifications.Tests;

public class NotificationDispatcherTests
{
    [Fact]
    public async Task SendEmailAsync_Resolves_INotificationSender_EmailMessage_From_DI()
    {
        var sender = new Mock<INotificationSender<EmailMessage>>();
        sender.Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var services = new ServiceCollection();
        services.AddScoped(_ => sender.Object);
        var sp = services.BuildServiceProvider();

        var dispatcher = new NotificationDispatcher(sp);
        var msg = new EmailMessage { To = new[] { "a@b" }, Subject = "S", Body = "B" };
        await dispatcher.SendEmailAsync(msg);  // extension method из Cheetah.Notifications.Email

        sender.Verify(s => s.SendAsync(msg, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendSmsAsync_Throws_When_Sender_Not_Registered()
    {
        var sp = new ServiceCollection().BuildServiceProvider();
        var dispatcher = new NotificationDispatcher(sp);

        var ex = await Should.ThrowAsync<InvalidOperationException>(
            () => dispatcher.SendSmsAsync(new SmsMessage("+79991234567", "test")).AsTask());
        ex.Message.ShouldContain("INotificationSender");
        ex.Message.ShouldContain("SmsMessage");
    }

    [Fact]
    public async Task SendAsync_Generic_Path_Resolves_By_TMessage_Type()
    {
        var sender = new Mock<INotificationSender<MyChannelMessage>>();
        var services = new ServiceCollection();
        services.AddScoped(_ => sender.Object);
        var dispatcher = new NotificationDispatcher(services.BuildServiceProvider());

        await dispatcher.SendAsync(new MyChannelMessage("payload"));

        sender.Verify(s => s.SendAsync(It.IsAny<MyChannelMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public record MyChannelMessage(string Payload);
}
