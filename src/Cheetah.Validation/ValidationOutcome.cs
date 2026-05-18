namespace Cheetah.Validation;

/// <summary>
/// Результат применения одного правила к одному значению.
/// IsValid=false означает нарушение; Code/Message детализируют его.
/// </summary>
public sealed record ValidationOutcome(bool IsValid, string? Code, string? Message)
{
    public static readonly ValidationOutcome Valid = new(true, null, null);
    public static ValidationOutcome Invalid(string code, string message) => new(false, code, message);
}
