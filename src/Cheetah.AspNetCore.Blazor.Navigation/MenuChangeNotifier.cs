using Cheetah.Core.DependencyInjection;

namespace Cheetah.AspNetCore.Blazor.Navigation;

/// <inheritdoc cref="IMenuChangeNotifier"/>
[Export(LifetimeType.Scoped, typeof(IMenuChangeNotifier))]
public class MenuChangeNotifier : IMenuChangeNotifier
{
    public event Func<Task>? MenuChanged;

    public async Task NotifyAsync()
    {
        var handlers = MenuChanged?.GetInvocationList();
        if (handlers is null)
            return;

        // Подписчиков обходим по одному и глушим их ошибки: меню может рисовать несколько
        // компонентов, и сбой одного не повод оставить остальные с устаревшим списком.
        foreach (var handler in handlers.Cast<Func<Task>>())
        {
            try
            {
                await handler();
            }
            catch
            {
                // Ошибку обрабатывает сам подписчик; здесь она не должна прерывать рассылку.
            }
        }
    }
}
