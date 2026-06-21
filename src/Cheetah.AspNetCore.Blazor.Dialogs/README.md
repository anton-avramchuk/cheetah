# Cheetah.AspNetCore.Blazor.Dialogs

Модуль модальных диалогов для Blazor-хоста (BFF). Поддерживает стандартные диалоги (alert, confirm) и кастомные — с произвольным компонентом, размерами и командами.

## Архитектура

```
IDialogService          ← точка входа для вызова диалогов
       ↓
DialogService           ← хранит стек ActiveDialog
       ↓ internal event
DialogHost.razor        ← рендерит весь стек диалогов
```

Диалоги складываются в стек — каждый новый открывается поверх предыдущего. Закрытие убирает конкретный диалог из стека; остальные остаются.

## Быстрый старт

Инжектируй `IDialogService` в компонент или сервис:

```csharp
@inject IDialogService DialogService
```

### Alert

```csharp
await DialogService.AlertAsync("Данные сохранены");
await DialogService.AlertAsync("Ошибка подключения", title: "Ошибка");
```

### Confirm

```csharp
if (await DialogService.ConfirmAsync("Удалить кандидата?"))
{
    await DeleteCandidateAsync();
}
```

### Кастомный диалог

```csharp
var result = await DialogService.ShowAsync(new DialogOptions
{
    Title       = "Новая вакансия",
    ContentType = typeof(VacancyFormComponent),
    ContentParameters = new() { ["Model"] = newVacancy },
    Width       = new DialogDimension.Percent(60),
    Height      = new DialogDimension.Pixels(500),
});

if (result.IsOk)
    await SaveAsync(newVacancy);
```

## DialogOptions

| Свойство | Тип | По умолчанию | Описание |
|---|---|---|---|
| `Title` | `string` | `""` | Заголовок диалога |
| `Message` | `string?` | `null` | Текст (используется если нет `ContentType`) |
| `ContentType` | `Type?` | `null` | Тип Razor-компонента для тела диалога |
| `ContentParameters` | `Dictionary<string, object?>?` | `null` | Параметры компонента |
| `Width` | `DialogDimension?` | `null` | Ширина окна |
| `Height` | `DialogDimension?` | `null` | Высота окна |
| `Commands` | `IReadOnlyList<IDialogCommand>` | `OkCancel` | Кнопки действий |

## DialogDimension

```csharp
new DialogDimension.Pixels(480)   // 480px
new DialogDimension.Percent(70)   // 70%
```

## Команды

### Встроенные

```csharp
DialogCommands.Ok        // кнопка "ОК", IsPrimary = true
DialogCommands.Cancel    // кнопка "Отмена", IsCancel = true
DialogCommands.OkOnly    // [Ok]
DialogCommands.OkCancel  // [Ok, Cancel]  ← дефолт
```

### Кастомная команда через делегат

```csharp
new DialogCommand("save", "Сохранить", IsPrimary: true, OnExecute: async ctx =>
{
    if (!form.IsValid) return;       // не закрываем — диалог остаётся
    await SaveAsync();
    await ctx.CloseAsync("save");    // явно закрываем
})
```

### Кастомная команда через IDialogCommand

Подходит для сложной логики или переиспользуемых команд:

```csharp
public class SaveVacancyCommand : IDialogCommand
{
    public string Id        => "save";
    public string Label     => "Сохранить";
    public bool IsPrimary   => true;
    public bool IsCancel    => false;

    public async Task ExecuteAsync(IDialogContext context)
    {
        var ok = await ValidateAndSaveAsync();
        if (ok)
            await context.CloseAsync(Id);
        // иначе диалог остаётся открытым
    }
}
```

Использование:
```csharp
await DialogService.ShowAsync(new DialogOptions
{
    Title    = "Вакансия",
    Commands = [new SaveVacancyCommand(), DialogCommands.Cancel]
});
```

## DialogResult

```csharp
var result = await DialogService.ShowAsync(...);

result.CommandId    // Id нажатой команды
result.IsOk         // true если CommandId == "ok"
result.IsCancelled  // true если CommandId == "cancel"
```

## Стек диалогов

Диалоги можно открывать не дожидаясь закрытия предыдущего — они накапливаются в стек:

```csharp
// открыть подтверждение и, не дожидаясь ответа, показать предупреждение поверх
var confirmTask = DialogService.ConfirmAsync("Применить изменения?");
await DialogService.AlertAsync("Сначала закройте это сообщение");

var confirmed = await confirmTask;
```

Фоновые диалоги визуально уменьшаются и уходят вглубь. Взаимодействие доступно только с верхним диалогом. Закрытие убирает конкретный диалог — остальные остаются на своих местах.

## Подключение в новом модуле

Если нужно использовать `IDialogService` из другого модуля, добавь зависимость:

```csharp
[DependsOn(typeof(CrmBlazorDialogsModule))]
public partial class MyModule : CrmModule { ... }
```

`DialogHost.razor` монтируется в layout-е (`Cheetah.AspNetCore.Blazor.Layouts` уже включает его в `MainLayout`); при собственном layout-е добавьте `<DialogHost />` один раз в корневой компонент.

## Зависимости

- `Cheetah.Core` (`CrmBlazorDialogsModule` → `[DependsOn(typeof(CoreModule))]`).
- `Microsoft.AspNetCore.App` (Razor-компонент `DialogHost`).
- DI генерируется из `[Export]`; `IDialogService`/`IDialogHost` — `Scoped`.
