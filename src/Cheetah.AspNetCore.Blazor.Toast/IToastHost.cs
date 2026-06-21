namespace Cheetah.AspNetCore.Blazor.Toast;

public interface IToastHost
{
    event Action<IReadOnlyList<ToastMessage>>? OnToastsChanged;
}
