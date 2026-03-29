namespace Cheetah.Core.Diagnostic.Attributes;

/// <summary>
/// Marks a method for execution time measurement. Requires the declaring service
/// to be registered via an interface and the module <see cref="CrmCoreDiagnosticModule"/> to be active.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public sealed class MeasureTimeAttribute : Attribute
{
    /// <summary>Optional label included in the log message.</summary>
    public string? Label { get; }

    public MeasureTimeAttribute(string? label = null)
    {
        Label = label;
    }
}
