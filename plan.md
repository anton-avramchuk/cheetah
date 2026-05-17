# План: подготовка фундамента под модуль Documents

> Цель: подготовить 5 общих компонентов, без которых модуль динамических документов
> писать преждевременно. Все компоненты должны работать одинаково и в монолите,
> и при выносе модулей в отдельные сервисы.

**Глобальные правила, применимые ко всем этапам:**

- Целевая платформа: **.NET 10.0**, C# 13, nullable enabled.
- Хранилище по умолчанию: **PostgreSQL** (`Npgsql.EntityFrameworkCore.PostgreSQL`).
- Версии пакетов — только через `Directory.Packages.props` (без `Version` в `.csproj`).
- Solution — `Cheetah.slnx`, проекты регистрируются через `dotnet sln add`.
- Все модули — partial-классы, наследники `CrmModule`, используют Source Generators.
- DI — через атрибут `[Export(LifetimeType.X, typeof(IY))]`.
- Тесты — xUnit + FluentAssertions + (где применимо) Testcontainers для Postgres.
- Хот-пасы используют `ValueTask<T>` и `CancellationToken`.
- События публикуются **только через Outbox**, обрабатываются **только через Inbox**.
- Контракты событий — **additive only**: добавлять nullable-поля, не переименовывать,
  не удалять. `[Obsolete]` для устаревших.
- Каждый модуль должен иметь README с примером подключения.

---

## Этап 1. Inbox (`Cheetah.Core.Inbox` + `Cheetah.Core.Inbox.PostgreSql`)

### 1.1 Зачем

Текущий Inbox-код живёт **внутри** `Cheetah.Core.Outbox` (`IInboxStore`,
`InboxMessage`, `InboxIdempotentEventHandler<T>`, `IdempotentAttribute`) и
привязан к нему через неявные зависимости. Нужно:

- Выделить Inbox в самостоятельную сборку (по образцу Outbox).
- Поддержать сценарий «модуль получает события из внешнего EventBus (Kafka),
  и обработчик должен быть идемпотентным» — обязательное условие для
  работы в микросервисах с at-least-once доставкой.
- Покрыть кейс множественных обработчиков одного события: каждый consumer
  имеет свою запись `(EventId, ConsumerName)`.
- Дать декоратор поверх любого `IEventHandler<T>`, который автоматически
  оборачивает его в idempotency-чек, без правки бизнес-кода.

### 1.2 Структура проектов

```
src/Cheetah.Core.Inbox/                       ← абстракции + декораторы (no infra)
src/Cheetah.Core.Inbox.EntityFrameworkCore/   ← EF Core реализация IInboxStore
src/Cheetah.Core.Inbox.PostgreSql/            ← Postgres-специфика (индексы, batching)
src/Cheetah.Core.Inbox.Tests/                 ← unit
src/Cheetah.Core.Inbox.Integration.Tests/     ← Testcontainers + Postgres
```

### 1.3 Ключевые контракты

- `IInboxStore` — `AlreadyProcessedAsync`, `AddAsync`, `DeleteOlderThanAsync`
  (перенести из `Cheetah.Core.Outbox` как есть, переименовать namespace).
- `InboxMessage` — `EventId`, `ConsumerName`, `EventType`, `ReceivedAt`,
  опционально `PayloadHash` для диагностики.
- `InboxIdempotentEventHandler<T>` — декоратор.
- `IInboxCleanupService` — фоновый сервис, периодически чистит старые записи.
- `InboxOptions` — `Retention` (default 30 дней), `CleanupInterval`,
  `CleanupBatchSize`.

### 1.4 Шаги реализации

- [ ] Создать `src/Cheetah.Core.Inbox/Cheetah.Core.Inbox.csproj`.
- [ ] Перенести из `Cheetah.Core.Outbox`:
      `IInboxStore`, `InboxMessage`, `InboxIdempotentEventHandler<T>`,
      `IdempotentAttribute`, `IdempotentHandlerRegistration`.
- [ ] В `Cheetah.Core.Outbox` оставить **type-forwarding** на старые типы
      или re-export через `[assembly: TypeForwardedTo(...)]`, чтобы не
      ломать существующих потребителей.
- [ ] Добавить `CrmInboxModule : CrmModule` с авто-регистрацией декораторов
      для всех `[Idempotent]`-помеченных handler'ов (через Source Generator
      из `Cheetah.Generators.Module`, проверить — возможно уже есть готовый).
- [ ] Реализовать `InboxOptions` + биндинг из `IConfiguration`.
- [ ] Реализовать `InboxCleanupService : BackgroundService`
      (использовать `Cheetah.BackgroundTasks`, если подходит).
- [ ] Создать `src/Cheetah.Core.Inbox.EntityFrameworkCore/`:
      перенести `EfInboxStore`, `InboxMessageConfiguration`,
      `ModelBuilderExtensions.AddInbox()`.
- [ ] Создать `src/Cheetah.Core.Inbox.PostgreSql/`:
      - `PostgresInboxOptimizedIndexSql` — uniq index на `(EventId, ConsumerName)`,
        partial index по `ProcessedAt IS NULL` для cleanup.
      - SQL-миграция для существующих БД.
- [ ] Добавить все новые проекты в `Cheetah.slnx`.
- [ ] Написать unit-тесты:
      - [ ] декоратор пропускает уже обработанные события
      - [ ] декоратор пишет в Inbox после успешной обработки
      - [ ] декоратор НЕ пишет в Inbox при исключении из inner handler
      - [ ] cleanup удаляет только старше `Retention`
      - [ ] параллельные дубликаты не приводят к двойной обработке
- [ ] Написать integration-тесты с Testcontainers Postgres:
      - [ ] уникальный индекс ловит race condition
      - [ ] cleanup корректно работает на больших объёмах (batch)
      - [ ] взаимодействие с реальным EventBus (Kafka в Testcontainers или
        FakeEventBus) — событие, доставленное дважды, обработано один раз
- [ ] Обновить README в `Cheetah.Core.Outbox` — пометить inbox-типы как
      перенесённые, дать ссылку на новый README.
- [ ] Написать README в `Cheetah.Core.Inbox` с примером:
      - подключение модуля,
      - регистрация DbContext с `modelBuilder.AddInbox()`,
      - использование `[Idempotent]` на handler'е,
      - настройка retention.

### 1.5 Совместимость монолит/микросервисы

- В **монолите** Inbox защищает от случайной двойной публикации через
  Redis EventBus (например, при retry'ах).
- В **микросервисах** Inbox — обязательное условие корректности при работе
  с Kafka (at-least-once).
- В тестах должны быть оба профиля: in-memory EventBus и Kafka.

### 1.6 Definition of Done

- [ ] Все unit + integration тесты зелёные.
- [ ] Один из существующих модулей (например, `Cheetah.Audit` или
      `Cheetah.Saga`) переведён на новый Inbox-модуль и продолжает работать.
- [ ] README актуален.
- [ ] Никаких breaking changes в публичном API `Cheetah.Core.Outbox`
      (type-forwarding работает).

---

## Этап 2. Expressions (`Cheetah.Expressions` + `Cheetah.Expressions.JsonLogic`)

### 2.1 Зачем

Единый движок выражений, который будет использоваться в трёх местах:

1. **Правила валидации** (`Cheetah.Validation`, этап 3) — кастомные правила.
2. **Guard'ы переходов state machine** (этап 5) — условия переходов.
3. **Условия и действия в Workflow-модуле** (после Documents).

Выражения должны быть:

- **Сериализуемыми** (хранятся в БД / JSON-схеме типа документа).
- **Безопасными** (никакого произвольного выполнения кода в MVP).
- **Детерминированными** (одинаковый ввод — одинаковый результат).
- **Тестируемыми** изолированно от бизнес-кода.

В качестве движка MVP — **JsonLogic** (https://jsonlogic.com), стандартный
формат, простая семантика, легко рендерится UI-редактором.

### 2.2 Структура проектов

```
src/Cheetah.Expressions/             ← абстракции IExpressionEvaluator
src/Cheetah.Expressions.JsonLogic/   ← реализация поверх JsonLogic
src/Cheetah.Expressions.Tests/
src/Cheetah.Expressions.JsonLogic.Tests/
```

### 2.3 Ключевые контракты

```csharp
public interface IExpressionEvaluator
{
    ValueTask<ExpressionResult<T>> EvaluateAsync<T>(
        string expression,
        IReadOnlyDictionary<string, object?> context,
        CancellationToken ct = default);

    ValueTask<bool> EvaluateBooleanAsync(
        string expression,
        IReadOnlyDictionary<string, object?> context,
        CancellationToken ct = default);

    ExpressionParseResult Parse(string expression);
}

public sealed record ExpressionResult<T>(bool Success, T? Value, string? Error);
public sealed record ExpressionParseResult(
    bool Valid, string? Error, IReadOnlyList<string> ReferencedVariables);
```

### 2.4 Шаги реализации

- [ ] Создать `Cheetah.Expressions` (только абстракции, без зависимостей
      кроме `Cheetah.Core`).
- [ ] Объявить `IExpressionEvaluator`, `ExpressionResult<T>`,
      `ExpressionParseResult`, `ExpressionException`.
- [ ] Объявить `IExpressionContext` — фасад над `IReadOnlyDictionary` с
      поддержкой dotted paths (`"document.amount"`, `"user.role"`).
- [ ] Создать `Cheetah.Expressions.JsonLogic`.
- [ ] Выбрать nuget: `JsonLogic` от gregsdennis (System.Text.Json,
      активно поддерживается). Добавить `PackageVersion` в
      `Directory.Packages.props`.
- [ ] Реализовать `JsonLogicExpressionEvaluator : IExpressionEvaluator`.
- [ ] Реализовать `CrmExpressionsJsonLogicModule : CrmModule` с регистрацией
      `IExpressionEvaluator` как Singleton.
- [ ] Поддержать кастомные операторы:
      - [ ] `"now"` — текущее время (для сравнения дат)
      - [ ] `"contains"` — для строк и массивов
      - [ ] `"regex"` — проверка по regex (с тайм-аутом!)
      - [ ] `"reference_exists"` — проверка существования id в справочнике
        (через DI-инжектируемый делегат, чтобы выражения могли спрашивать
        «существует ли контрагент с таким id»)
- [ ] Добавить лимиты безопасности:
      - [ ] `MaxExpressionLength` (default 4096)
      - [ ] `MaxEvaluationTime` (default 100 ms, через CancellationToken)
      - [ ] `MaxNestingDepth` (default 32)
- [ ] Добавить все проекты в `Cheetah.slnx`.
- [ ] Unit-тесты:
      - [ ] базовые операции (`==`, `!=`, `>`, `<`, `>=`, `<=`, `and`, `or`, `not`)
      - [ ] арифметика (`+`, `-`, `*`, `/`, `%`)
      - [ ] доступ к вложенным полям через `var` с dotted-path
      - [ ] работа с null (null-safe, не падает)
      - [ ] кастомные операторы (now, contains, regex, reference_exists)
      - [ ] лимиты срабатывают (длина, глубина, тайм-аут)
      - [ ] Parse возвращает список переменных
      - [ ] Parse корректно ловит синтаксические ошибки
- [ ] README с примерами выражений:
      - `{"==": [{"var": "status"}, "Draft"]}`
      - `{"and": [{">": [{"var": "amount"}, 100000]}, {"==": [{"var": "category"}, "VIP"]}]}`

### 2.5 Совместимость монолит/микросервисы

Stateless, без I/O — работает одинаково. `reference_exists` использует
`IReferenceLookup` (DI), который в монолите идёт в локальную БД,
в микросервисах — в client-библиотеку соответствующего сервиса.

### 2.6 Definition of Done

- [ ] Все тесты зелёные, покрытие основных операций ≥ 90%.
- [ ] README с 5+ примерами выражений.
- [ ] Бенчмарк: ≥ 100k вычислений в секунду на одном ядре для простых
      выражений (sanity check на отсутствие O(n²) внутри).

---

## Этап 3. Validation (`Cheetah.Validation`)

### 3.1 Зачем

Унифицированный фреймворк декларативных правил валидации, который:

- Покрывает типовые проверки (Required, StringLength, Range, Pattern, ...).
- Поддерживает кросс-полевые правила через **Expressions** (этап 2).
- Возвращает **список ошибок**, не исключение (для UI).
- Сериализуется в JSON (правила хранятся в схеме типа документа).
- Транслируется в OpenAPI-схему (фронт получает constraints автоматически).

Применяется не только в Documents — это общий инфраструктурный компонент
уровня FluentValidation, но заточенный под динамические данные.

### 3.2 Структура проектов

```
src/Cheetah.Validation/         ← абстракции, базовые правила, ValidationContext
src/Cheetah.Validation.Tests/
```

### 3.3 Ключевые контракты

```csharp
public interface IValidationRule
{
    string RuleType { get; }
    ValueTask<ValidationOutcome> ValidateAsync(
        ValidationContext context,
        CancellationToken ct = default);
}

public sealed record ValidationContext(
    string FieldPath,
    object? Value,
    IReadOnlyDictionary<string, object?> AllValues,
    IServiceProvider Services);

public sealed record ValidationOutcome(bool IsValid, string? Code, string? Message);
public sealed record ValidationError(string FieldPath, string Code, string Message);
public sealed record ValidationResult(bool IsValid, IReadOnlyList<ValidationError> Errors);

public interface IValidationEngine
{
    ValueTask<ValidationResult> ValidateAsync(
        IReadOnlyList<(string FieldPath, object? Value,
                       IReadOnlyList<IValidationRule> Rules)> fields,
        IReadOnlyDictionary<string, object?> allValues,
        CancellationToken ct = default);
}
```

### 3.4 Встроенные правила

- [ ] `RequiredRule` — значение не null и (для строк) не пустое.
- [ ] `StringLengthRule(min?, max?)`.
- [ ] `NumberRangeRule(min?, max?)` — int/long/decimal/double.
- [ ] `DateRangeRule(min?, max?)`.
- [ ] `PatternRule(regex)` — regex с тайм-аутом.
- [ ] `EnumValueRule(allowedValues)`.
- [ ] `CompareRule(otherFieldPath, op)` — сравнение с другим полем
      (например, `EndDate > StartDate`).
- [ ] `RequiredIfRule(expression)` — обязательно, если выражение истинно.
- [ ] `ExpressionRule(expression, message)` — произвольное JsonLogic-выражение.
- [ ] `UniqueRule(scope)` — placeholder в MVP (реализация будет в Documents,
      т.к. требует доступа к репозиторию документов).

### 3.5 Шаги реализации

- [ ] Создать `Cheetah.Validation` со ссылкой на `Cheetah.Core` и
      `Cheetah.Expressions`.
- [ ] Определить контракты (см. 3.3).
- [ ] Реализовать каждое правило из 3.4 как отдельный класс,
      пометить `[JsonPolymorphic]` для (де)сериализации.
- [ ] Реализовать `ValidationEngine : IValidationEngine` — последовательно
      применяет правила, агрегирует ошибки.
- [ ] Реализовать `CrmValidationModule : CrmModule` с регистрацией
      движка и встроенных правил.
- [ ] Реализовать `IValidationRuleSerializer` — Json (де)сериализация
      набора правил (для хранения в схеме типа).
- [ ] Реализовать `IOpenApiSchemaExtender` — конвертер правил → OpenAPI
      constraints (`minLength`, `maxLength`, `pattern`, `minimum`, `maximum`).
- [ ] Добавить проекты в `Cheetah.slnx`.
- [ ] Unit-тесты для каждого правила (≥ 3 теста: happy path, граница,
      нарушение).
- [ ] Unit-тесты для движка:
      - [ ] все ошибки агрегируются, не падает на первой
      - [ ] кросс-полевые правила видят весь словарь значений
      - [ ] `ExpressionRule` корректно использует `IExpressionEvaluator`
- [ ] Unit-тесты для сериализации:
      - [ ] roundtrip каждого правила (rule → json → rule)
      - [ ] неизвестный тип правила даёт понятную ошибку
- [ ] README с примерами:
      ```json
      [
        {"$type": "Required"},
        {"$type": "StringLength", "min": 3, "max": 100}
      ]
      ```

### 3.6 Совместимость монолит/микросервисы

Stateless. Правила могут потребовать I/O только в `UniqueRule` и кастомных
правилах через Expressions — для них применяются те же принципы, что в этапе 2.

### 3.7 Definition of Done

- [ ] Все правила покрыты тестами.
- [ ] Сериализация/десериализация работает для всех встроенных типов.
- [ ] README с примерами.
- [ ] Один из существующих модулей (например, Identity или Customer)
      использует `IValidationEngine` для валидации команды — proof of concept.

---

## Этап 4. Permissions (`Cheetah.Permissions`)

### 4.1 Зачем

RBAC-модуль уровня самого Documents. Нужен **до** Documents, потому что:

- Переходы состояний потребуют прав (`"Documents.Contract.Sign"`).
- Действия (assign, reject, archive) — тоже.
- Видимость полей и фильтрация документов — потенциально тоже.

Должен работать **локально** в монолите и **через HTTP-клиент** в микросервисах,
с одинаковым интерфейсом `IPermissionsClient`.

### 4.2 Структура проектов

```
src/Modules/Permissions/
  ├── Cheetah.Permissions.Events/         ← UserRoleAssignedEvent, RoleCreatedEvent, ...
  ├── Cheetah.Permissions.Shared/         ← PermissionConstants, BuiltInRoles
  ├── Cheetah.Permissions.Contracts/      ← UserPermissionsDto, PermissionCheckRequest
  ├── Cheetah.Permissions.Domain/         ← Role, Permission, UserRoleAssignment, Specs
  ├── Cheetah.Permissions.Application/    ← CQRS: AssignRole, CheckPermission, ListUserRoles
  ├── Cheetah.Permissions.DataAccess/     ← EF Core + Postgres
  ├── Cheetah.Permissions.Api/            ← Minimal API
  ├── Cheetah.Permissions.Client/         ← IPermissionsClient (HTTP + Local impls)
  └── Tests/
       ├── Cheetah.Permissions.Domain.Tests/
       ├── Cheetah.Permissions.Application.Tests/
       └── Cheetah.Permissions.Client.Tests/
```

### 4.3 Модель

- `Permission` — строковый ключ типа `"Documents.Contract.Sign"` +
  description. Хранится как seed-данные (объявляется кодом, регистрируется
  в БД при старте).
- `Role` — именованный набор `Permission`. Создаётся пользователем или
  кодом (BuiltInRoles: `"Admin"`, `"User"`).
- `UserRoleAssignment` — связь `(UserId, RoleId, Scope?)`. Scope —
  опциональный контекст (например, `tenant:42` или `documentType:Contract`)
  — нужен под мульти-арендность и тонкие гранты.
- Все агрегаты — `AggregateRoot<Guid>`.

### 4.4 Ключевые контракты

```csharp
public interface IPermissionsClient
{
    ValueTask<bool> HasPermissionAsync(
        Guid userId, string permission, string? scope = null,
        CancellationToken ct = default);

    ValueTask<IReadOnlyList<string>> GetUserPermissionsAsync(
        Guid userId, CancellationToken ct = default);

    ValueTask AssignRoleAsync(Guid userId, Guid roleId, string? scope, CancellationToken ct);
    ValueTask RevokeRoleAsync(Guid userId, Guid roleId, string? scope, CancellationToken ct);
}

public interface IAuthorizationService
{
    ValueTask<AuthorizationResult> AuthorizeAsync(
        ClaimsPrincipal user, string permission, string? scope = null,
        CancellationToken ct = default);
}
```

### 4.5 Шаги реализации

- [ ] Создать структуру проектов из 4.2, добавить в `Cheetah.slnx`.
- [ ] **Events**: `RoleCreatedEvent`, `RoleDeletedEvent`,
      `PermissionGrantedToRoleEvent`, `UserRoleAssignedEvent`,
      `UserRoleRevokedEvent`.
- [ ] **Shared**:
      - [ ] `PermissionConstants`, `BuiltInRoles`.
      - [ ] `PermissionRegistrationAttribute` — для декларативного объявления
        `[Permission("Documents.Contract.Sign", "Подписать договор")]`
        в любом модуле.
- [ ] **Contracts**: DTO для API + Client.
- [ ] **Domain**:
      - [ ] `Permission`, `Role`, `UserRoleAssignment` агрегаты.
      - [ ] Спецификации: `PermissionByKeySpec`, `RoleByNameSpec`,
        `UserAssignmentsByUserSpec`, `UserAssignmentByUserRoleSpec`.
      - [ ] Repository интерфейсы: `IRoleRepository`,
        `IUserRoleAssignmentRepository`, `IPermissionRepository`.
- [ ] **Application**:
      - [ ] Commands: `CreateRoleCommand`, `DeleteRoleCommand`,
        `GrantPermissionToRoleCommand`, `RevokePermissionFromRoleCommand`,
        `AssignRoleToUserCommand`, `RevokeRoleFromUserCommand`.
      - [ ] Queries: `CheckUserPermissionQuery`, `GetUserPermissionsQuery`,
        `GetUserRolesQuery`, `ListRolesQuery`.
      - [ ] `IUserPermissionsCache` (in-memory с TTL 1-5 минут) +
        инвалидация на события `UserRoleAssignedEvent`/`Revoked`
        через Inbox.
- [ ] **DataAccess**:
      - [ ] `PermissionsDbContext`, конфигурации, миграции.
      - [ ] Реализации репозиториев.
      - [ ] Регистрация в DI через `[Export]`.
- [ ] **Api** (Minimal API):
      - [ ] `POST /api/permissions/check`
      - [ ] `GET /api/users/{id}/permissions`
      - [ ] `POST /api/roles`, `DELETE /api/roles/{id}`
      - [ ] `POST /api/roles/{id}/permissions`, `DELETE /api/roles/{id}/permissions/{key}`
      - [ ] `POST /api/users/{id}/roles`, `DELETE /api/users/{id}/roles/{roleId}`
- [ ] **Client**:
      - [ ] `IPermissionsClient` — общий интерфейс.
      - [ ] `LocalPermissionsClient` — обращается напрямую к `IDispatcher`
        (для монолита).
      - [ ] `HttpPermissionsClient` — `HttpClient` к Permissions.Api
        (для микросервисов).
      - [ ] Регистрация в DI: профиль по конфигу `Permissions:Mode`
        (`"Local"` | `"Remote"`).
      - [ ] `IAuthorizationService` — поверх `IPermissionsClient`,
        тянет userId из `ClaimsPrincipal`.
- [ ] Source Generator для `[Permission]`-атрибута:
      - [ ] сканирует сборки,
      - [ ] собирает список permission-ключей,
      - [ ] синхронизирует с БД при старте Permissions-модуля.
- [ ] Seed BuiltInRoles при старте модуля (`OnApplicationInitialization`).
- [ ] **Тесты**:
      - [ ] Domain.Tests — инварианты агрегатов, спецификации.
      - [ ] Application.Tests — handler'ы, кэш, инвалидация на событиях.
      - [ ] Client.Tests — оба клиента (Http через `WebApplicationFactory`,
        Local через in-memory DI).
- [ ] README:
      - [ ] подключение модуля,
      - [ ] объявление permission через атрибут,
      - [ ] использование `IAuthorizationService` в Application,
      - [ ] переключение режима Local/Remote.

### 4.6 Совместимость монолит/микросервисы

Главное достижение этапа — **один и тот же бизнес-код использует
`IPermissionsClient`** независимо от того, где живёт модуль Permissions.
DI решает по конфигу. Это шаблон для всех будущих модулей, включая Documents.

### 4.7 Definition of Done

- [ ] Все тесты зелёные.
- [ ] Permissions работает в обоих режимах (есть интеграционный тест
      в обоих).
- [ ] Хотя бы один существующий модуль (Identity?) использует
      `IAuthorizationService` для команды.
- [ ] Кэш + инвалидация подтверждены тестом.
- [ ] README актуален.

---

## Этап 5. StateMachine data-driven extension

### 5.1 Зачем

Текущий `Cheetah.Core.StateMachine` — code-first, генерик по enum,
fluent. Для динамических документов нужно описывать состояния и переходы
**данными** (из БД). При этом не ломать существующее API: code-first типы
документов и любые другие сущности должны продолжать работать.

### 5.2 Структура проектов

```
src/Cheetah.Core.StateMachine/           ← существующий, обогащается контрактом
src/Cheetah.Core.StateMachine.Dynamic/   ← новая сборка, runtime-определяемые FSM
src/Cheetah.Core.StateMachine.Dynamic.Tests/
```

(Раздельные сборки нужны: code-first вариант не должен тянуть EF Core /
Postgres / репозитории — он работает на in-memory табличках переходов.)

### 5.3 Унифицирующий контракт

```csharp
public interface IStateMachine<TState>
{
    bool CanTransition(TState from, TState to);
    IReadOnlyCollection<TState> GetAllowedTargets(TState from);
}

public interface IDynamicStateMachine
{
    ValueTask<bool> CanTransitionAsync(
        Guid machineDefinitionId,
        string fromState,
        string toState,
        IReadOnlyDictionary<string, object?> context,
        CancellationToken ct = default);

    ValueTask<IReadOnlyList<string>> GetAllowedTargetsAsync(
        Guid machineDefinitionId,
        string fromState,
        IReadOnlyDictionary<string, object?> context,
        CancellationToken ct = default);

    ValueTask<TransitionResult> TransitionAsync(
        DynamicTransitionRequest request,
        CancellationToken ct = default);
}

public sealed record DynamicTransitionRequest(
    Guid MachineDefinitionId,
    string FromState,
    string ToState,
    IReadOnlyDictionary<string, object?> Context,
    Guid? ActingUserId);

public sealed record TransitionResult(
    bool Success,
    string? FailureReason,
    IReadOnlyList<string> ExecutedActions);
```

### 5.4 Модель данных (Dynamic)

- `StateMachineDefinition` (AggregateRoot)
  - `Id`, `Name`, `Version`, `InitialState`
  - `States: StateDefinition[]` (имя, isFinal, requiredFields?)
  - `Transitions: TransitionDefinition[]` (from, to, guardExpression?,
    requiredPermission?, actions[])
- `TransitionAction` — `Type` (`"SendNotification"`, `"CreateRelatedRecord"`,
  `"CallWebhook"`, `"StartSaga"`) + параметры (JSON).

### 5.5 Шаги реализации

#### 5.5.1 Обогащение существующего модуля

- [ ] Вытащить общий контракт `IStateMachine<TState>` в
      `Cheetah.Core.StateMachine`.
- [ ] Адаптировать существующий `StateMachineBuilder<TState>` так, чтобы
      результирующий объект реализовывал `IStateMachine<TState>`
      (тонкий wrapper).
- [ ] Не ломать публичное API — все существующие потребители
      (например, `Cheetah.Saga.EntityFrameworkCore`) должны компилироваться.
- [ ] Unit-тесты на новый интерфейс поверх code-first FSM.

#### 5.5.2 Новая сборка Dynamic

- [ ] Создать `Cheetah.Core.StateMachine.Dynamic` (зависит от
      `Cheetah.Core.StateMachine`, `Cheetah.Expressions`,
      `Cheetah.Permissions.Client`, `Cheetah.Core.Outbox`).
- [ ] Объявить модель: `StateMachineDefinition`, `StateDefinition`,
      `TransitionDefinition`, `TransitionAction`.
- [ ] Объявить репозиторий `IStateMachineDefinitionRepository` +
      спецификации (`ByIdSpec`, `ByNameSpec`).
- [ ] Реализовать `DynamicStateMachine : IDynamicStateMachine`:
      - [ ] Загружает определение из репозитория (с кэшем).
      - [ ] При проверке перехода:
        - найти `TransitionDefinition` для `(from, to)`,
        - проверить permission через `IPermissionsClient`,
        - вычислить `guardExpression` через `IExpressionEvaluator`,
        - если ок — вернуть `Allowed`.
      - [ ] При выполнении перехода (атомарно):
        - все проверки выше,
        - записать новое состояние + аудит + публикация события в Outbox
          + регистрация actions в очередь выполнения,
        - actions выполняются **после** транзакции через Outbox-consumer.
- [ ] Реализовать `ITransitionAction` + встроенные действия:
      - [ ] `SendNotificationAction` (через `Cheetah.Notifications`)
      - [ ] `CallWebhookAction` (через Outbox)
      - [ ] `StartSagaAction` (через `Cheetah.Saga`)
      - [ ] (`CreateDocumentAction` — placeholder, будет добавлено
        после появления Documents)
- [ ] `ITransitionActionExecutor` — диспетчер action'ов, подбирает
      реализацию по `Type`.
- [ ] Publish `DynamicStateTransitionedEvent(machineDefinitionId, entityId,
      fromState, toState, occurredAt, byUserId)`.
- [ ] Регистрация модуля `CrmStateMachineDynamicModule`.
- [ ] EF Core конфигурации + миграция для таблиц определений.
- [ ] **Кэш определений**: in-memory с инвалидацией по событию
      `StateMachineDefinitionUpdatedEvent` (через Inbox).
- [ ] Добавить проекты в `Cheetah.slnx`.

#### 5.5.3 Тесты

- [ ] Unit-тесты на `DynamicStateMachine`:
      - [ ] переход разрешён, если есть `TransitionDefinition` без guard'а
        и без permission
      - [ ] переход запрещён, если guard вернул false
      - [ ] переход запрещён, если у пользователя нет permission
      - [ ] actions запускаются только после успешной транзакции
      - [ ] actions НЕ запускаются, если транзакция упала
- [ ] Интеграционные тесты (Postgres + in-memory EventBus):
      - [ ] конкурентный переход одной сущности — выигрывает только один
        (через `Cheetah.DistributedLock` или optimistic concurrency)
      - [ ] после успешного перехода `DynamicStateTransitionedEvent`
        реально попадает в Outbox
- [ ] Тесты actions:
      - [ ] `SendNotificationAction` обращается к `INotificationDispatcher`
      - [ ] `CallWebhookAction` ставит сообщение в Outbox
      - [ ] неизвестный тип action даёт понятную ошибку

#### 5.5.4 Документация

- [ ] README в `Cheetah.Core.StateMachine.Dynamic` с примером:
      - создание определения через API,
      - JSON-структура определения,
      - выполнение перехода из Application-кода,
      - кастомное action через `ITransitionAction`.

### 5.6 Совместимость монолит/микросервисы

- `IDynamicStateMachine` локален — определения и переходы хранятся в
  БД того модуля, который владеет сущностью (Documents в будущем).
- Permissions и Expressions работают через свои абстракции —
  они уже совместимы.
- Action `StartSagaAction` использует `Cheetah.Saga`, который сам
  совместим с обоими режимами.

### 5.7 Definition of Done

- [ ] Code-first FSM продолжает работать без изменений у потребителей.
- [ ] Dynamic FSM покрыт unit + integration тестами.
- [ ] Демонстрационный пример: создан тестовый workflow «Approval» из 3
      состояний с guard'ом и permission, отработан end-to-end.
- [ ] README актуален.

---

## После всех 5 этапов

Когда все галочки выше проставлены — можно переходить к проектированию
**`Cheetah.Documents`**, опираясь на готовые:

- `Cheetah.Core.Inbox` — идемпотентная обработка событий схемы и переходов.
- `Cheetah.Expressions.JsonLogic` — guard'ы переходов, кастомные правила
  валидации, условия в action'ах.
- `Cheetah.Validation` — правила валидации полей.
- `Cheetah.Permissions` — RBAC на переходах и операциях с документами.
- `Cheetah.Core.StateMachine.Dynamic` — собственно workflow документов.

Documents становится «тонкой» сборкой, главное содержимое которой —
агрегаты `DocumentType` и `Document`, репозитории с JSONB-стратегией
и Application-слой. Вся «нагруженная» инфра уже снаружи.

---

## Сквозные правила (повтор для надёжности)

- [ ] Все события — additive, через Outbox, обрабатываются через Inbox.
- [ ] Cross-module-вызовы — только через `*.Client`-библиотеки и общие
      интерфейсы, никаких прямых ссылок на DataAccess других модулей.
- [ ] Профиль `Local` / `Remote` выбирается конфигом, не кодом.
- [ ] Каждый модуль регистрируется в `Cheetah.slnx`.
- [ ] Версии пакетов — только в `Directory.Packages.props`.
- [ ] Каждая публичная API-сборка имеет README с примером подключения.
