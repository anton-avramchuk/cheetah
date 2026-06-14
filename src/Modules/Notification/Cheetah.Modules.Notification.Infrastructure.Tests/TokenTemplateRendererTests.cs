using Cheetah.Modules.Notification.Infrastructure.Rendering;
using Cheetah.Modules.Notification.Shared;
using Shouldly;

namespace Cheetah.Modules.Notification.Infrastructure.Tests;

public class TokenTemplateRendererTests
{
    private readonly TokenTemplateRenderer _renderer = new();

    [Fact]
    public void HasTemplate_True_ForKnownKeyAndChannel()
        => _renderer.HasTemplate("order.shipped", NotificationChannel.Email).ShouldBeTrue();

    [Fact]
    public void HasTemplate_False_ForUnknownKey()
        => _renderer.HasTemplate("does.not.exist", NotificationChannel.Email).ShouldBeFalse();

    [Fact]
    public void HasTemplate_False_ForChannelWithoutTemplate()
        => _renderer.HasTemplate("order.shipped", NotificationChannel.Sms).ShouldBeFalse();

    [Fact]
    public void Render_SubstitutesTokens()
    {
        var result = _renderer.Render("order.shipped", NotificationChannel.Email,
            new Dictionary<string, string> { ["user_name"] = "Анна", ["order_number"] = "A-100" });

        result.Subject.ShouldBe("Заказ A-100 отправлен");
        result.Body.ShouldContain("Анна");
        result.Body.ShouldContain("A-100");
    }

    [Fact]
    public void Render_MissingToken_ReplacedWithEmpty_NoLeftoverBraces()
    {
        var result = _renderer.Render("order.shipped", NotificationChannel.Email,
            new Dictionary<string, string>()); // данных нет

        result.Subject.ShouldBe("Заказ  отправлен"); // токен вырезан, двойной пробел
        result.Subject.ShouldNotContain("{{");
        result.Body.ShouldNotContain("{{");
    }

    [Fact]
    public void Render_UnknownTemplate_Throws()
        => Should.Throw<InvalidOperationException>(() => _renderer.Render(
            "nope", NotificationChannel.Email, new Dictionary<string, string>()));
}
