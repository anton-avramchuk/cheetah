namespace Cheetah.AspNetCore.Blazor.Layouts;

/// <summary>
/// Прячет Blazor-страницу за фич-флагом — аналог серверного
/// <c>routes.MapGet(...).RequireFeature("key")</c> (тот отвечает 404) для роутинга компонентов.
/// <para>
/// Работает только через <see cref="FeatureGate"/> в <c>Routes.razor</c>: сам по себе атрибут ничего
/// не гейтит. Выключенная фича → страница рендерится как «не найдено», то есть её как будто нет.
/// </para>
/// <example><code>
/// @attribute [RequireFeature("Vacancy.Teams")]
/// </code></example>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RequireFeatureAttribute : Attribute
{
    public RequireFeatureAttribute(string feature)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(feature);
        Feature = feature;
    }

    /// <summary>Ключ фич-флага, например <c>"Vacancy.Teams"</c>.</summary>
    public string Feature { get; }
}
