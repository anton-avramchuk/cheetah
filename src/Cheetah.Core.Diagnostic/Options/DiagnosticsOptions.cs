namespace Cheetah.Core.Diagnostic.Options;

public class DiagnosticsOptions
{
    public const string SectionName = "Diagnostics";

    /// <summary>
    /// Master switch. When <c>false</c>, disables all diagnostic features
    /// (timing, request logging) regardless of their individual settings.
    /// </summary>
    public bool Enabled { get; set; } = true;
}
