namespace Cheetah.AspNetCore.Blazor.Navigation;

/// <summary>
/// Сообщает, что меню нужно перестроить. Нужен меню, чьи пункты приходят из данных (например,
/// список вакансий пользователя): такие пункты меняются в течение сессии, а компонент меню
/// строит его один раз при инициализации.
/// <para>
/// Живёт в scope пользователя (Blazor-цепь), поэтому уведомление получают только компоненты
/// текущей сессии.
/// </para>
/// </summary>
public interface IMenuChangeNotifier
{
    /// <summary>Подписка компонентов меню на пересборку.</summary>
    event Func<Task>? MenuChanged;

    /// <summary>Просит подписчиков перестроить меню.</summary>
    Task NotifyAsync();
}
