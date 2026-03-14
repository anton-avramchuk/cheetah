# Cheetah.BackgroundTasks

Инфраструктурный модуль для фоновых задач. Поддерживает периодический запуск по `TimeSpan` и расписание по cron-выражению.

## Зависимости

| Пакет | Назначение |
|---|---|
| `Cheetah.Core` | модульная система, `CrmModule` |
| `Cronos` | парсинг cron-выражений |
| `Microsoft.Extensions.Hosting.Abstractions` | `BackgroundService` |

## Подключение

```csharp
[DependsOn(typeof(CrmBackgroundTasksModule))]
public partial class MyApiBootstrapperModule : CrmModule { }
```

`BackgroundTaskScheduler` регистрируется как `IHostedService` автоматически.

---

## Абстракции

### `IBackgroundTask`

Маркерный интерфейс. Все задачи, зарегистрированные в DI как `IBackgroundTask`, подхватываются планировщиком.

```
IBackgroundTask
└── BackgroundTask (abstract)
    ├── PeriodicBackgroundTask (abstract) — обязательное свойство Period : TimeSpan
    └── CronBackgroundTask (abstract)    — обязательное свойство CronExpression : string
```

### `PeriodicBackgroundTask`

Запускается через равные промежутки времени с помощью `PeriodicTimer` (.NET 6+).

| Свойство | Тип | Обязательно | Описание |
|---|---|---|---|
| `Period` | `TimeSpan` | **да** | Интервал между запусками |
| `InitialDelay` | `TimeSpan` | нет | Задержка перед первым запуском (по умолчанию `TimeSpan.Zero`) |
| `Name` | `string` | нет | Имя задачи в логах (по умолчанию — имя класса) |

### `CronBackgroundTask`

Запускается по cron-расписанию. Следующий момент запуска вычисляется через `Cronos`.

| Свойство | Тип | Обязательно | Описание |
|---|---|---|---|
| `CronExpression` | `string` | **да** | Cron-выражение (5 или 6 полей) |
| `TimeZone` | `TimeZoneInfo` | нет | Часовой пояс (по умолчанию UTC) |
| `Format` | `CronFormat` | нет | `Standard` (5 полей) или `IncludeSeconds` (6 полей) |
| `Name` | `string` | нет | Имя задачи в логах (по умолчанию — имя класса) |

---

## Примеры

### Периодическая задача

```csharp
[Export(LifetimeType.Singleton, typeof(IBackgroundTask))]
public class CleanupTask : PeriodicBackgroundTask
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CleanupTask(IServiceScopeFactory scopeFactory) =>
        _scopeFactory = scopeFactory;

    // Запускается каждый час
    public override TimeSpan Period => TimeSpan.FromHours(1);

    // Первый запуск — через 30 секунд после старта приложения
    public override TimeSpan InitialDelay => TimeSpan.FromSeconds(30);

    public override async Task ExecuteAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
        await repo.DeleteExpiredAsync(ct);
    }
}
```

### Cron-задача

```csharp
[Export(LifetimeType.Singleton, typeof(IBackgroundTask))]
public class DailyReportTask : CronBackgroundTask
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DailyReportTask(IServiceScopeFactory scopeFactory) =>
        _scopeFactory = scopeFactory;

    // Каждый день в 08:00 по московскому времени
    public override string CronExpression => "0 8 * * *";
    public override TimeZoneInfo TimeZone =>
        TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");

    public override async Task ExecuteAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IReportService>();
        await service.GenerateDailyReportAsync(ct);
    }
}
```

### Cron с секундами

```csharp
[Export(LifetimeType.Singleton, typeof(IBackgroundTask))]
public class HeartbeatTask : CronBackgroundTask
{
    // Каждые 30 секунд
    public override string CronExpression => "*/30 * * * * *";
    public override CronFormat Format => CronFormat.IncludeSeconds;

    public override Task ExecuteAsync(CancellationToken ct)
    {
        // ...
        return Task.CompletedTask;
    }
}
```

---

## Жизненный цикл

```
Старт приложения
    └─► BackgroundTaskScheduler.ExecuteAsync()
            ├─► Task 1 (PeriodicBackgroundTask) ──► InitialDelay ──► loop: WaitForNextTick ──► ExecuteAsync
            ├─► Task 2 (CronBackgroundTask)     ──► WaitUntilNext ──► ExecuteAsync ──► WaitUntilNext ──► ...
            └─► Task N ...

Остановка приложения
    └─► CancellationToken отменяется
            ├─► PeriodicTimer завершает цикл
            └─► Task.Delay (cron) бросает OperationCanceledException → задача завершается
```

## Обработка ошибок

- Исключения внутри `ExecuteAsync` **логируются** и **проглатываются** — задача продолжает работу по расписанию.
- `OperationCanceledException` **пробрасывается** — обеспечивает корректную остановку.
- Невалидное cron-выражение логируется как `Error`, задача **не запускается** (не падает весь хост).

## Работа со scoped-зависимостями

Задачи регистрируются как **Singleton** (требование `IHostedService`). Для доступа к scoped-сервисам (например, `DbContext`, репозитории) нужно создавать scope вручную:

```csharp
public override async Task ExecuteAsync(CancellationToken ct)
{
    using var scope = _scopeFactory.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    // работа с db...
}
```

## Справка по cron-выражениям

| Выражение | Описание |
|---|---|
| `* * * * *` | Каждую минуту |
| `0 * * * *` | Каждый час |
| `0 8 * * *` | Каждый день в 08:00 |
| `0 8 * * 1` | Каждый понедельник в 08:00 |
| `0 0 1 * *` | Первое число каждого месяца |
| `*/5 * * * *` | Каждые 5 минут |
| `0 0 * * * *` | Каждый час (6-field с секундами) |
| `*/30 * * * * *` | Каждые 30 секунд (6-field) |
