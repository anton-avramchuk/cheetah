using Cheetah.Modules.Notification.Application.Services;
using Cheetah.Modules.Notification.Domain.Entities;
using Cheetah.Modules.Notification.Shared;
using Shouldly;

namespace Cheetah.Modules.Notification.Application.Tests;

public class DefaultChannelRouterTests
{
    private readonly DefaultChannelRouter _router = new();

    private static RecipientContact Contact(string? email = null, string? phone = null, string? push = null)
        => RecipientContact.Create(Guid.NewGuid(), email, phone, push);

    [Fact]
    public void NoContact_ReturnsEmptyPlan()
        => _router.Resolve(NotificationCategory.Transactional, contact: null, forceChannel: null)
            .ShouldBeEmpty();

    [Fact]
    public void Transactional_WithEmailOnly_ReturnsEmail()
        => _router.Resolve(NotificationCategory.Transactional, Contact(email: "a@b.c"), null)
            .ShouldBe(new[] { NotificationChannel.Email });

    [Fact]
    public void Transactional_WithEmailAndPhone_PrefersEmailThenSms()
        => _router.Resolve(NotificationCategory.Transactional, Contact(email: "a@b.c", phone: "+1"), null)
            .ShouldBe(new[] { NotificationChannel.Email, NotificationChannel.Sms });

    [Fact]
    public void Otp_PrefersSmsOverEmail()
        => _router.Resolve(NotificationCategory.Otp, Contact(email: "a@b.c", phone: "+1"), null)
            .ShouldBe(new[] { NotificationChannel.Sms, NotificationChannel.Email });

    [Fact]
    public void Marketing_OnlyEmail()
        => _router.Resolve(NotificationCategory.Marketing, Contact(email: "a@b.c", phone: "+1"), null)
            .ShouldBe(new[] { NotificationChannel.Email });

    [Fact]
    public void ForceChannel_Honored_WhenContactSupportsIt()
        => _router.Resolve(NotificationCategory.Transactional, Contact(email: "a@b.c"), NotificationChannel.Email)
            .ShouldBe(new[] { NotificationChannel.Email });

    [Fact]
    public void ForceChannel_Empty_WhenContactLacksThatChannel()
        => _router.Resolve(NotificationCategory.Transactional, Contact(email: "a@b.c"), NotificationChannel.Sms)
            .ShouldBeEmpty();

    [Fact]
    public void ChannelsWithoutMatchingContact_AreFilteredOut()
        => _router.Resolve(NotificationCategory.System, Contact(email: "a@b.c"), null)
            .ShouldNotContain(NotificationChannel.Push); // System = [Email, Push], но push-токена нет
}
