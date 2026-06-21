using Cheetah.Core.DependencyInjection;

namespace Cheetah.AspNetCore.Blazor.Toast;

[Export(LifetimeType.Scoped, typeof(IToastService))]
[Export(LifetimeType.Scoped, typeof(IToastHost))]
public class ToastService : IToastService, IToastHost, IDisposable
{
    internal event Action<IReadOnlyList<ToastMessage>>? OnToastsChanged;

    event Action<IReadOnlyList<ToastMessage>>? IToastHost.OnToastsChanged
    {
        add    => OnToastsChanged += value;
        remove => OnToastsChanged -= value;
    }

    private readonly List<ToastMessage> _toasts = new();
    private readonly Dictionary<Guid, CancellationTokenSource> _timers = new();

    public void Show(ToastMessage message)
    {
        _toasts.Add(message);
        Notify();

        if (message.Duration > TimeSpan.Zero)
            ScheduleDismiss(message);
    }

    public void Success(string message, string? title = null, TimeSpan? duration = null)
        => Show(new ToastMessage { Type = ToastType.Success, Title = title, Message = message, Duration = duration ?? TimeSpan.FromSeconds(4) });

    public void Info(string message, string? title = null, TimeSpan? duration = null)
        => Show(new ToastMessage { Type = ToastType.Info, Title = title, Message = message, Duration = duration ?? TimeSpan.FromSeconds(4) });

    public void Warning(string message, string? title = null, TimeSpan? duration = null)
        => Show(new ToastMessage { Type = ToastType.Warning, Title = title, Message = message, Duration = duration ?? TimeSpan.FromSeconds(5) });

    public void Error(string message, string? title = null, TimeSpan? duration = null)
        => Show(new ToastMessage { Type = ToastType.Error, Title = title, Message = message, Duration = duration ?? TimeSpan.Zero });

    public void Dismiss(Guid id)
    {
        if (_timers.Remove(id, out var cts))
            cts.Cancel();

        _toasts.RemoveAll(t => t.Id == id);
        Notify();
    }

    private void ScheduleDismiss(ToastMessage toast)
    {
        var cts = new CancellationTokenSource();
        _timers[toast.Id] = cts;

        Task.Delay(toast.Duration, cts.Token).ContinueWith(t =>
        {
            if (!t.IsCanceled) Dismiss(toast.Id);
        }, TaskScheduler.Default);
    }

    private void Notify() => OnToastsChanged?.Invoke(_toasts.AsReadOnly());

    public void Dispose()
    {
        foreach (var cts in _timers.Values)
            cts.Cancel();
        _timers.Clear();
    }
}
