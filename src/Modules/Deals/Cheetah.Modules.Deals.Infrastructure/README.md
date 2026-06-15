# Cheetah.Modules.Deals.Infrastructure

Инфраструктура Deals: EF Core, миграции, репозитории и Outbox. Зависит на Domain +
`CrmEntityFrameworkModule`/`…PostgreSqlModule` + `CrmOutbox*`.

## Состав

| Компонент | Назначение |
|---|---|
| `DealsDbContext` | `CrmDbContext`; реализует `IOutboxDbContext` + `IDeadLetterDbContext`. Схема `deals` |
| `DealsDbContextFactory` | Design-time фабрика для `dotnet ef migrations` |
| Конфигурации | `Pipeline`/`PipelineStage`, `Deal` (owned `Money` → `Amount`/`Currency`), `DealStageHistory`; `Ignore(DomainEvents)`, индексы, каскады |
| Репозитории | `DealRepository` (`IDealRepository`), `PipelineRepository` (`IPipelineRepository`), `DealStageHistoryRepository` — на `EfRepository<…>` |
| Миграции | `Initial` — таблицы + Outbox/DeadLetter |

## Outbox

Интеграционные события сделок публикуются через `IEventBus` в `OutboxMessages` того же
`DealsDbContext` — атомарно с сохранением агрегата (`AddPostgresOutboxStore`/`AddDeadLetterStore`).

## Миграции

```bash
cd src/Modules/Deals/Cheetah.Modules.Deals.Infrastructure
dotnet ef migrations add <Name> --output-dir Persistence/Migrations
```
