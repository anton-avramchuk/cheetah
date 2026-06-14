using System.Text.RegularExpressions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Notification.Domain.Abstractions;
using Cheetah.Modules.Notification.Shared;

namespace Cheetah.Modules.Notification.Infrastructure.Rendering;

/// <summary>
/// Рендерер шаблонов без внешних зависимостей: подстановка merge-полей вида <c>{{ token }}</c>.
///
/// <para>Изначально планировался Scriban, но он флагается критическими уязвимостями во всех версиях
/// без патча (см. NU1903/NU1904), поэтому полноценный движок-песочница убран. Для доверенных
/// шаблонов нам нужна только подстановка значений — синтаксис <c>{{ name }}</c> сохранён, чтобы
/// при появлении безопасного движка заменить реализацию без правки шаблонов.</para>
///
/// <para>Каталог шаблонов — статический in-memory (ключ "{templateKey}:{channel}"). В реальной
/// системе переедет в БД/конфиг с локализацией — follow-up.</para>
/// </summary>
[Export(LifetimeType.Singleton, typeof(ITemplateRenderer))]
public sealed partial class TokenTemplateRenderer : ITemplateRenderer
{
    private sealed record TemplateDefinition(string Subject, string Body);

    private static readonly IReadOnlyDictionary<string, TemplateDefinition> Catalog =
        new Dictionary<string, TemplateDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["order.shipped:Email"] = new(
                Subject: "Заказ {{ order_number }} отправлен",
                Body: "<p>Здравствуйте, {{ user_name }}!</p><p>Ваш заказ <b>{{ order_number }}</b> передан в доставку.</p>"),
            ["auth.otp:Email"] = new(
                Subject: "Код подтверждения",
                Body: "<p>Ваш код: <b>{{ code }}</b>. Никому его не сообщайте.</p>")
        };

    [GeneratedRegex(@"\{\{\s*([a-zA-Z0-9_\.]+)\s*\}\}")]
    private static partial Regex TokenRegex();

    public bool HasTemplate(string templateKey, NotificationChannel channel)
        => Catalog.ContainsKey(Key(templateKey, channel));

    public RenderedMessage Render(string templateKey, NotificationChannel channel, IReadOnlyDictionary<string, string> data)
    {
        var key = Key(templateKey, channel);
        if (!Catalog.TryGetValue(key, out var definition))
            throw new InvalidOperationException($"Template '{key}' not found");

        return new RenderedMessage(Substitute(definition.Subject, data), Substitute(definition.Body, data));
    }

    private static string Substitute(string template, IReadOnlyDictionary<string, string> data)
        => TokenRegex().Replace(template, match =>
        {
            var token = match.Groups[1].Value;
            return data.TryGetValue(token, out var value) ? value : string.Empty;
        });

    private static string Key(string templateKey, NotificationChannel channel) => $"{templateKey}:{channel}";
}
