using Cheetah.AspNetCore.Blazor.Navigation;

namespace Cheetah.AspNetCore.Blazor.Tests.Navigation;

/// <summary>
/// Пересборка меню по требованию. Нужна меню с данными: список из БД (например, вакансии
/// пользователя) меняется в течение сессии, а меню строится один раз при инициализации NavMenu.
/// </summary>
public class MenuChangeNotifierTests
{
    [Fact]
    public async Task NotifyAsync_InvokesSubscribers()
    {
        var notifier = new MenuChangeNotifier();
        var calls = 0;
        notifier.MenuChanged += () =>
        {
            calls++;
            return Task.CompletedTask;
        };

        await notifier.NotifyAsync();

        calls.ShouldBe(1);
    }

    [Fact]
    public async Task NotifyAsync_InvokesEverySubscriber()
    {
        var notifier = new MenuChangeNotifier();
        var first = 0;
        var second = 0;
        notifier.MenuChanged += () => { first++; return Task.CompletedTask; };
        notifier.MenuChanged += () => { second++; return Task.CompletedTask; };

        await notifier.NotifyAsync();

        first.ShouldBe(1);
        second.ShouldBe(1);
    }

    [Fact]
    public async Task NotifyAsync_WithoutSubscribers_DoesNotThrow()
        => await new MenuChangeNotifier().NotifyAsync();

    [Fact]
    public async Task Unsubscribed_HandlerIsNotInvoked()
    {
        var notifier = new MenuChangeNotifier();
        var calls = 0;
        Func<Task> handler = () => { calls++; return Task.CompletedTask; };

        notifier.MenuChanged += handler;
        notifier.MenuChanged -= handler;
        await notifier.NotifyAsync();

        calls.ShouldBe(0);
    }

    /// <summary>
    /// Упавший подписчик не должен мешать остальным: меню рисуют несколько компонентов, и сбой
    /// одного не повод оставить другие с устаревшим списком.
    /// </summary>
    [Fact]
    public async Task FailingSubscriber_DoesNotBlockOthers()
    {
        var notifier = new MenuChangeNotifier();
        var reached = 0;
        notifier.MenuChanged += () => throw new InvalidOperationException("boom");
        notifier.MenuChanged += () => { reached++; return Task.CompletedTask; };

        await notifier.NotifyAsync();

        reached.ShouldBe(1);
    }
}
