# Cheetah.Modules.Teams.* — абстрактный шаблон-модуль «Команды и состав»

> **Тип:** base/template (каркас), а не готовый микросервис.
> Модуль поставляет **абстрактную расширяемую команду** + конкретные справочники ролей/участников и
> generic-хелперы. Конкретное приложение наследует тип команды, генерирует миграции у себя и получает
> рабочий CRUD команд/ролей/участников + управление составом «из коробки», дописав ~6 классов. Сделан
> по образцу [`Cheetah.Modules.Catalog`](../Catalog/README.md).

## Назначение

Кто над чем работает: команды/отделы/группы (расширяемый `TeamBase`), справочник ролей
(`TeamRole`), справочник людей (`TeamMember`) и их членство (`TeamMembership` — участник + роль в
команде). Команда — агрегат, владеющий составом; инвариант «один участник — одна роль в команде»
держится внутри агрегата.

**Главное требование — расширяемость:** команда и её ViewModel наследуемы; приложение добавляет свои
поля (отдел, цвет, аватар) без форка модуля. Роли/участники — конкретные справочники.

## Состав сборок и граф зависимостей

```
Teams.DomainEvents   → Core.Events                          (TeamCreated/Updated/Deactivated/Activated, MemberAdded/Removed/RoleChanged)
Teams.Shared         → Core                                 (TeamsConstants: подключение, длины, схема/таблицы, префиксы маршрутов)
Teams.Contracts      → Core + Contracts + Shared            (ABSTRACT TeamDtoBase/RequestBase + конкретные DTO ролей/участников)
Teams.Domain         → DomainEvents + Specification         (abstract TeamBase; sealed TeamRole/TeamMember/TeamMembership; спеки)
Teams.Infrastructure → Domain + EF + EF.PostgreSql + Grid + Identity.Client   (abstract TeamsDbContextBase/TeamConfigurationBase, AddTeamsInfrastructure<>, фоновый синк участников из Identity)
Teams.Application    → Domain + Contracts + CQRS + Events    (generic CQRS команды, [Export]-хендлеры ролей/участников, AddTeamsApplication<>)
Teams.Api            → Application + Contracts + Backend.Endpoints   (декларативные эндпоинты)
Teams.Mapster        → Application + Contracts + Mapping.Mapster     (TeamsMappingProfile — роли/участники)
Tests: Domain.Tests (16), Application.Tests (21)
```

`Default` и `Client` отсутствуют — их создаёт наследник / следующий инкремент (нужны закрытые типы).

## Ключевые абстракции

| Тип | Сборка | Роль |
|---|---|---|
| `TeamBase : AggregateRoot<Guid>` | Domain | агрегат команды со составом; `InitializeCore`, `Rename`, `Activate`/`Deactivate`, `AddMember`/`ChangeMemberRole`/`RemoveMember` |
| `TeamMembership : Entity<Guid>` | Domain | членство (дитя команды): `MemberId`, `RoleId` |
| `TeamRole : AggregateRoot<Guid>` | Domain | справочник ролей (sealed): `Create`/`Rename` |
| `TeamMember : AggregateRoot<Guid>` | Domain | справочник людей = реплика пользователей Identity (sealed): `Id` == id пользователя, `Name`, `SyncHash`; `CreateFromDirectory`/`Apply` (синк по хэшу) |
| `IIdentityUserDirectory` / `UserDirectoryEntry` | Domain | порт к Identity + снимок пользователя с `ComputeHash()` |
| `ITeamMemberDirectorySynchronizer` | Domain→App | оркестратор bulk-синка (апсёрт только изменившихся по хэшу) |
| `TeamMemberDirectorySyncService` | Infrastructure | фоновый `BackgroundService`, периодически вызывает синхронизатор |
| `ITeamFactory`/`ITeamProjector` | Application | `Create(...)`/`ToDto(...)` — замена `new`/Mapster для расширяемой команды |
| generic CQRS команды | Application | Create/Update/Activate/Deactivate/Delete + AddMember/ChangeRole/RemoveMember + GetById/List/Grid |
| `[Export]`-хендлеры ролей/участников | Application | полный CRUD + Grid |
| `TeamsDbContextBase<TContext, TTeam>` | Infrastructure | `DbSet` команд + ролей + участников + членств |
| `TeamConfigurationBase<TTeam>` | Infrastructure | таблица/схема, уникальное имя, коллекция состава (`AutoInclude`), `ConfigureCustom` hook |
| декларативные эндпоинты | Api | наследники `Cheetah.Backend.Endpoints` + генератор; грид через `IGridRepository` |

## Extension-методы (точки регистрации у наследника)

```csharp
// Infrastructure: DbContext, мигратор, PostgreSQL, grid-репозитории команды/ролей/участников
services.AddTeamsInfrastructure<AppTeamsDbContext, Team>();

// Application: фабрика, проектор, закрытые generic CQRS-handler'ы команды
services.AddTeamsApplication<Team, CreateTeamRequest, UpdateTeamRequest,
    TeamDto, TeamGridViewModel, TeamFactory, TeamProjector>();
```

## Эндпоинты

**Роли и участники — конкретные, работают «из коробки»:**

| Метод | Маршрут | Назначение |
|---|---|---|
| POST/GET/GET{id}/PUT/DELETE | `api/team-roles` | CRUD + грид ролей |
| GET/GET{id} | `api/team-members` | грид + деталь участников (только чтение — наполняются из Identity) |

**Команды — абстрактные шаблоны** (команда расширяема) → закрывает наследник/хост:

| Шаблон | База | Маршрут (по умолчанию) |
|---|---|---|
| `CreateTeamEndpoint<TRequest,TCommand>` | CreateCommandEndpoint | POST `api/teams` |
| `GetTeamByIdEndpoint<TRequest,TQuery,TDto>` | QueryOrNotFoundEndpoint | GET `api/teams/{id}` |
| `UpdateTeamEndpoint<TRequest,TCommand>` | UpdateCommandEndpoint | PUT `api/teams/{id}` |
| `DeleteTeamEndpoint<TRequest,TCommand>` | DeleteCommandEndpoint | DELETE `api/teams/{id}` |
| `GetTeamsGridEndpoint<TRequest,TQuery,TGridVm>` | QueryGridEndpoint | GET `api/teams` |
| `AddTeamMemberEndpoint<TRequest,TCommand>` | CommandEndpoint | POST `api/teams/{id}/members` |
| `ChangeTeamMemberRoleEndpoint<TRequest,TCommand>` | UpdateCommandEndpoint | PUT `api/teams/{id}/members/{memberId}` |
| `RemoveTeamMemberEndpoint<TRequest,TCommand>` | DeleteCommandEndpoint | DELETE `api/teams/{id}/members/{memberId}` |

## Синхронизация участников из Identity

`TeamMember` — локальная реплика пользователя Identity (идентификаторы совпадают). Актуальность
поддерживает фоновый `TeamMemberDirectorySyncService` (`BackgroundService`): периодически тянет всех
пользователей через порт `IIdentityUserDirectory` (адаптер над `IIdentityUsersClient`, `[Export]`) и
апсёртит в справочник. Чтобы **не нагружать БД**, у `TeamMember` есть `SyncHash` — SHA-256 синкаемого
содержимого: запись обновляется только если хэш снимка изменился (`TeamMember.Apply`), неизменившиеся
строки не пишутся вовсе. Набор синкаемых полей знает только `UserDirectoryEntry.ComputeHash()` —
добавление поля не требует правок в синке. Исчезнувшие из Identity участники **удаляются** (пруннинг);
при этом пустой ответ источника пруннинг не запускает — защита от массового удаления при
недоступности Identity. Настройки — `TeamMemberSyncOptions` (секция `Teams:MemberSync`): `Enabled`,
`RunOnStartup`, `Interval` (по умолчанию 5 мин), `PruneRemoved` (по умолчанию `true`).

> Bulk-синк по образцу модуля Tags (`UserDirectorySyncService`). Поддержку по доменным событиям
> Identity (`UserNameChanged`/`UserDeleted`) можно добавить как follow-up. Пруннинг удаляет строку
> `TeamMember`, но не чистит `TeamMembership`, ссылающиеся на удалённого участника (FK нет) — очистку
> «осиротевших» членств можно добавить отдельным шагом.

## События

`TeamCreated/Updated/Deactivated/Activated`, `TeamMemberAdded/Removed/RoleChanged`
(`*IntegrationEvent`, `Cheetah.Core.Events`). Потребители: Permissions (доступ по команде),
Notifications, Timeline, аналитика.

## Ограничения / follow-up

- Миграции и `IDesignTimeDbContextFactory` — только у наследника (в модуле их нет).
- `Client` и готовая сборка `.Default` — следующий инкремент.
- Кросс-агрегатная валидация (существование `MemberId`/`RoleId` при добавлении в команду) — follow-up
  (сейчас, как `SetPrice` в Catalog, не проверяется).
- Транзакционный Outbox — follow-up (сейчас публикация после `SaveChangesAsync`).
- Коды ответов: not-found маппится в 400 (`TeamsValidationException`) — уточнить до 404.
