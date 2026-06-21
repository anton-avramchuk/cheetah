# Cheetah.AspNetCore.Blazor.Toast

Модуль toast-уведомлений для Blazor-хоста (BFF). Поддерживает четыре типа, автодисмисс с прогресс-баром, ручное закрытие и персистентные сообщения.

## Быстрый старт

```csharp
@inject IToastService Toast

Toast.Success("Кандидат сохранён");
Toast.Error("Ошибка подключения к базе данных", title: "Ошибка");
Toast.Warning("Вакансия не заполнена", duration: TimeSpan.FromSeconds(8));
Toast.Info("Данные обновляются...", duration: TimeSpan.Zero); // персистентный
```

## Типы уведомлений

| Метод | Тип | Иконка | Duration по умолчанию |
|---|---|---|---|
| `Success` | `ToastType.Success` | ✓ зелёный | 4 с |
| `Info` | `ToastType.Info` | ℹ синий | 4 с |
| `Warning` | `ToastType.Warning` | ⚠ жёлтый | 5 с |
| `Error` | `ToastType.Error` | ✕ красный | **∞ (персистентный)** |

Ошибки по умолчанию не закрываются автоматически — пользователь должен их явно прочитать и закрыть.

## API

```csharp
// Короткие методы
Toast.Success(string message, string? title = null, TimeSpan? duration = null);
Toast.Info   (string message, string? title = null, TimeSpan? duration = null);
Toast.Warning(string message, string? title = null, TimeSpan? duration = null);
Toast.Error  (string message, string? title = null, TimeSpan? duration = null);

// Полный контроль
Toast.Show(new ToastMessage
{
    Type     = ToastType.Success,
    Title    = "Готово",
    Message  = "Импорт завершён: 142 записи",
    Duration = TimeSpan.FromSeconds(6)
});

// Ручной дисмисс по Id
var msg = new ToastMessage { ... };
Toast.Show(msg);
// ...
Toast.Dismiss(msg.Id);
```

## ToastMessage

| Свойство | Тип | По умолчанию | Описание |
|---|---|---|---|
| `Id` | `Guid` | `NewGuid()` | Уникальный идентификатор |
| `Type` | `ToastType` | `Info` | Тип уведомления |
| `Title` | `string?` | `null` | Заголовок (опционально) |
| `Message` | `string` | `""` | Текст сообщения |
| `Duration` | `TimeSpan` | `4s` | Время показа; `Zero` = до ручного закрытия |

## Поведение

- Уведомления накапливаются в правом верхнем углу, новые появляются снизу стека
- Каждый toast с ненулевым `Duration` показывает убывающий прогресс-бар
- Кнопка `×` запускает анимацию исчезновения (250 мс), затем удаляет
- Автодисмисс прерывается при ручном закрытии (CancellationToken)
- `IToastService` зарегистрирован как `Scoped` — состояние изолировано по пользовательской сессии

## Подключение в новом модуле

```csharp
[DependsOn(typeof(CrmBlazorToastModule))]
public partial class MyModule : CrmModule { ... }
```

`ToastHost.razor` монтируется в layout-е (`Cheetah.AspNetCore.Blazor.Layouts` уже включает его в `MainLayout`); если используете собственный layout — добавьте `<ToastHost />` один раз в корневой компонент.

## Зависимости

- `Cheetah.Core` (`CrmBlazorToastModule` → `[DependsOn(typeof(CoreModule))]`).
- `Microsoft.AspNetCore.App` (Razor-компонент `ToastHost`).
- DI генерируется из `[Export]` (`Cheetah.Generators.Module`); `IToastService`/`IToastHost` — `Scoped`.
