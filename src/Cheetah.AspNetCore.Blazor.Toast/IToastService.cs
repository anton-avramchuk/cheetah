namespace Cheetah.AspNetCore.Blazor.Toast;

public interface IToastService
{
    void Show(ToastMessage message);
    void Success(string message, string? title = null, TimeSpan? duration = null);
    void Info(string message, string? title = null, TimeSpan? duration = null);
    void Warning(string message, string? title = null, TimeSpan? duration = null);
    void Error(string message, string? title = null, TimeSpan? duration = null);
    void Dismiss(Guid id);
}
