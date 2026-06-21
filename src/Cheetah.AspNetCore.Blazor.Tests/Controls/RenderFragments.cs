using Microsoft.AspNetCore.Components;

namespace Cheetah.AspNetCore.Blazor.Tests.Controls;

/// <summary>Хелперы для сборки RenderFragment в bUnit-тестах контролов.</summary>
internal static class RenderFragments
{
    /// <summary>RenderFragment из сырой HTML-разметки.</summary>
    public static RenderFragment Html(string markup) => builder => builder.AddMarkupContent(0, markup);
}
