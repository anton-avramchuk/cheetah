namespace Cheetah.AspNetCore.Blazor.Toast;

public record ToastMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public ToastType Type { get; init; } = ToastType.Info;
    public string? Title { get; init; }
    public string Message { get; init; } = string.Empty;

    /// <summary>Zero = persistent until manually dismissed.</summary>
    public TimeSpan Duration { get; init; } = TimeSpan.FromSeconds(4);
}
