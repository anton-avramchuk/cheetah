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

- [x] Создать `src/Cheetah.Core.Inbox/Cheetah.Core.Inbox.csproj`.
- [x] Перенести из `Cheetah.Core.Outbox`:
      `IInboxStore`, `InboxMessage`, `InboxIdempotentEventHandler<T>`,
      `IdempotentAttribute`, `IdempotentHandlerRegistration`.
- [x] ~~В `Cheetah.Core.Outbox` оставить type-forwarding~~ — type-forwarding
      требует идентичного FQN, что несовместимо с новым namespace `Cheetah.Core.Inbox`.
      Выбран чистый перенос с обновлением use-sites в проекте.
- [x] Добавить `CrmInboxModule : CrmModule` с авто-регистрацией декораторов
      для всех `[Idempotent]`-помеченных handler'ов (через существующий
      Source Generator `Cheetah.Generators.Module`, обновлены константы namespace).
- [x] Реализовать `InboxOptions` + биндинг из `IConfiguration`.
- [x] Реализовать `InboxCleanupService : BackgroundService`.
- [x] Создать `src/Cheetah.Core.Inbox.EntityFrameworkCore/`:
      перенести `EfInboxStore`, `InboxMessageConfiguration`,
      `ModelBuilderExtensions.AddInbox()`, `IInboxDbContext`, `AddInboxStore<T>()`.
- [x] Создать `src/Cheetah.Core.Inbox.PostgreSql/`:
      - `InboxOptimizedIndexSql` — индекс по `ReceivedAt` для cleanup.
      - Уникальный составной PK `(EventId, ConsumerName)` уже задаётся
        в `InboxMessageConfiguration` и ловит race conditions.
- [x] Добавить все новые проекты в `Cheetah.slnx`.
- [x] Написать unit-тесты:
      - [x] декоратор пропускает уже обработанные события
      - [x] декоратор пишет в Inbox после успешной обработки
      - [x] декоратор НЕ пишет в Inbox при исключении из inner handler
      - [x] cleanup удаляет только старше `Retention`
      - [x] cleanup делает no-op без зарегистрированного `IInboxStore`
      - [x] cleanup использует корректный threshold = now - retention
      - [x] Source Generator оборачивает `[Idempotent]`-handler в декоратор
      - [x] повторное событие не вызывает inner-handler
- [x] Написать integration-тесты с Testcontainers Postgres:
      - [x] PK на `(EventId, ConsumerName)` ловит race condition (DbUpdateException)
      - [x] `AlreadyProcessedAsync` корректно различает consumer'ов
      - [x] `DeleteOlderThanAsync` удаляет только старше threshold, оставляя recent
      - [x] `DeleteOlderThanAsync` уважает batchSize
      - [x] End-to-end: повторная доставка через декоратор + EfInboxStore
        приводит к одному вызову inner-handler'а (имитация at-least-once)
- [x] Обновить README в `Cheetah.Core.Outbox` — пометить inbox-типы как
      перенесённые, дать ссылку на новый README.
- [x] Написать README в `Cheetah.Core.Inbox` с примером:
      - подключение модуля,
      - регистрация DbContext с `modelBuilder.AddInbox()`,
      - использование `[Idempotent]` на handler'е,
      - настройка retention.
- [x] Обновить README в `Cheetah.Core.Outbox.EntityFrameworkCore`.
- [x] Написать README в `Cheetah.Core.Inbox.EntityFrameworkCore` и
      `Cheetah.Core.Inbox.PostgreSql`.
- [x] Обновить `Cheetah.Generators.Module/ModuleServicesGenerator.cs` —
      изменить константы namespace на `Cheetah.Core.Inbox.*`.
- [x] Убрать Inbox-логику из `OutboxCleanupService` (теперь Inbox чистит
      собственный `InboxCleanupService`).
- [x] Убрать `IInboxDbContext` из `TestDbContext` в `Cheetah.Core.Outbox.Integration.Tests`.

### 1.5 Совместимость монолит/микросервисы

- В **монолите** Inbox защищает от случайной двойной публикации через
  Redis EventBus (например, при retry'ах).
- В **микросервисах** Inbox — обязательное условие корректности при работе
  с Kafka (at-least-once).
- В тестах должны быть оба профиля: in-memory EventBus и Kafka.

### 1.6 Definition of Done

- [x] Все unit + integration тесты зелёные:
      - `Cheetah.Core.Inbox.Tests` — 8 passed.
      - `Cheetah.Core.Inbox.Integration.Tests` — 6 passed (Postgres через Testcontainers).
      - `Cheetah.Core.Outbox.Tests` — 10 passed (после удаления Inbox-тестов).
      - `Cheetah.Core.Outbox.Integration.Tests` — 9 passed.
      - Полный билд солюшна — 0 errors.
- [ ] ~~Один из существующих модулей переведён на новый Inbox-модуль~~ —
      существующих потребителей `[Idempotent]` нет (раньше тесты `Cheetah.Core.Outbox.Tests`
      содержали единственное использование). Будет проверено при первом
      реальном применении (например, в `Cheetah.Workflow` после Documents).
- [x] README актуален: `Cheetah.Core.Inbox`, `Cheetah.Core.Inbox.EntityFrameworkCore`,
      `Cheetah.Core.Inbox.PostgreSql`, обновлены `Cheetah.Core.Outbox` и
      `Cheetah.Core.Outbox.EntityFrameworkCore`.
- [x] **Breaking change по namespace принят осознанно:** `Cheetah.Core.Outbox.IInboxStore`
      и связанные типы переехали в `Cheetah.Core.Inbox.*`. Type-forwarding
      технически невозможен (требует идентичный FQN). Все use-sites в проекте обновлены.

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

- [x] Создать `Cheetah.Expressions` (только абстракции, без зависимостей
      кроме `Cheetah.Core`).
- [x] Объявить `IExpressionEvaluator`, `ExpressionResult<T>`,
      `ExpressionParseResult`, `ExpressionException`, `ExpressionLimitExceededException`.
- [x] Объявить `IExpressionContext` + реализацию `DictionaryExpressionContext`
      с поддержкой dotted paths (`"document.amount"`, `"user.role"`).
- [x] Объявить `IReferenceLookup` для оператора `reference_exists`.
- [x] Создать `Cheetah.Expressions.JsonLogic`.
- [x] Выбрать nuget: `JsonLogic 6.1.0` от gregsdennis (System.Text.Json,
      активно поддерживается). Добавлен `PackageVersion` в `Directory.Packages.props`.
- [x] Реализовать `JsonLogicExpressionEvaluator : IExpressionEvaluator`.
- [x] Реализовать `CrmExpressionsJsonLogicModule : CrmModule` с регистрацией
      `IExpressionEvaluator` как Singleton.
- [x] Реализовать расширенные операторы через pre-resolve (`ExpressionPreprocessor`),
      а не как кастомные JsonLogic-rules — это позволяет не зависеть от внутреннего
      реестра правил библиотеки и тестировать каждый оператор отдельно:
      - [x] `"now"` — sync, заменяется на ISO-8601 timestamp при каждом вычислении
      - [x] `"regex"` — sync, проверка с timeout-защитой от ReDoS; `var` разворачивается из контекста
      - [x] `"reference_exists"` — async, через `IReferenceLookup`,
        вызывается отдельно `ExpressionPreprocessor.ResolveReferencesAsync`
      - [ ] ~~`"contains"`~~ — не нужен, в JsonLogic есть встроенный `"in"` для строк и массивов
- [x] Добавить лимиты безопасности:
      - [x] `MaxExpressionLength` (default 4096)
      - [x] `MaxEvaluationTime` (default 100 ms, через CancellationToken)
      - [x] `MaxNestingDepth` (default 32)
      - [x] `RegexTimeout` (default 50 ms)
- [x] Добавить все проекты в `Cheetah.slnx`.
- [x] Unit-тесты:
      - [x] базовые операции (`==`, `!=`, `>`, `<`, `>=`, `<=`, `and`, `or`, `!`)
      - [x] арифметика (`+`, `-`, `*`, `/`, `%`)
      - [x] доступ к вложенным полям через `var` с dotted-path
      - [x] работа с null (null-safe, не падает)
      - [x] расширенные операторы (`now`, `regex` с literal и с `var`)
      - [x] `reference_exists` через mock IReferenceLookup (true/false/missing context)
      - [x] лимиты срабатывают (длина, глубина)
      - [x] Parse возвращает список переменных (включая форму `var: ["foo", "default"]`)
      - [x] Parse корректно ловит синтаксические ошибки
      - [x] `EvaluateBooleanAsync` возвращает false для невалидных и нон-bool результатов
      - [x] `DictionaryExpressionContext` — все варианты dotted-path
- [x] README с примерами выражений:
      - `{"==": [{"var": "status"}, "Draft"]}`
      - `{"and": [{">": [{"var": "amount"}, 100000]}, {"==": [{"var": "category"}, "VIP"]}]}`
      - `{"<": [{"var": "deadline"}, {"now": []}]}`
      - `{"regex": ["^\\+7\\d{10}$", {"var": "phone"}]}`
      - `{"reference_exists": ["Counterparties", {"var": "document.counterpartyId"}]}`

### 2.5 Совместимость монолит/микросервисы

Stateless, без I/O — работает одинаково. `reference_exists` использует
`IReferenceLookup` (DI), который в монолите идёт в локальную БД,
в микросервисах — в client-библиотеку соответствующего сервиса.

### 2.6 Definition of Done

- [x] Все тесты зелёные:
      - `Cheetah.Expressions.Tests` — 6 passed (DictionaryExpressionContext).
      - `Cheetah.Expressions.JsonLogic.Tests` — 44 passed (evaluator + preprocessor).
      - Полный билд солюшна — 0 errors.
- [x] README актуален в `Cheetah.Expressions` и `Cheetah.Expressions.JsonLogic` с 5+ примерами.
- [ ] ~~Бенчмарк ≥ 100k/s~~ — отложен. Sanity check: единичные вычисления простых
      выражений в тестах укладываются в миллисекунды; полноценный benchmark
      будет добавлен при появлении первого реального потребителя (Validation/StateMachine).

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

- [x] `RequiredRule` — значение не null и (для строк) не пустое.
- [x] `StringLengthRule(min?, max?)`.
- [x] `NumberRangeRule(min?, max?)` — int/long/decimal/double/строковые числа.
- [x] `DateRangeRule(min?, max?)` — DateTime/DateTimeOffset/ISO-строка.
- [x] `PatternRule(regex)` — regex с тайм-аутом 50 мс.
- [x] `EnumValueRule(allowedValues)`.
- [x] `CompareRule(otherFieldPath, op)` — Eq/Ne/Gt/Ge/Lt/Le.
- [x] `RequiredIfRule(expression)` — через `IExpressionEvaluator`.
- [x] `ExpressionRule(expression, message)` — через `IExpressionEvaluator`.
- [x] `UniqueRule(scope)` — placeholder no-op (реализация в Documents).

### 3.5 Шаги реализации

- [x] Создать `Cheetah.Validation` со ссылкой на `Cheetah.Core` и
      `Cheetah.Expressions`.
- [x] Определить контракты (см. 3.3).
- [x] Реализовать каждое правило из 3.4 как отдельный класс,
      пометить `[JsonPolymorphic]` на `IValidationRule` через `[JsonDerivedType]`.
- [x] Реализовать `ValidationEngine : IValidationEngine` — последовательно
      применяет правила, агрегирует ошибки.
- [x] Реализовать `CrmValidationModule : CrmModule` с регистрацией
      движка и сериализатора.
- [x] Реализовать `IValidationRuleSerializer` — Json (де)сериализация
      набора правил (для хранения в схеме типа).
- [ ] ~~`IOpenApiSchemaExtender`~~ — отложен до появления первого реального
      потребителя (Documents.Api), чтобы понять точную схему OpenAPI-extension'а.
- [x] Добавить проекты в `Cheetah.slnx`.
- [x] Unit-тесты для каждого правила (≥ 3 теста: happy path, граница, нарушение).
- [x] Unit-тесты для движка:
      - [x] все ошибки агрегируются, не падает на первой
      - [x] кросс-полевые правила видят весь словарь значений
      - [x] `ExpressionRule` корректно использует `IExpressionEvaluator`
      - [x] Engine использует `Code` из `ValidationOutcome`
- [x] Unit-тесты для сериализации:
      - [x] roundtrip каждого правила (rule → json → rule)
      - [x] неизвестный тип правила даёт `JsonException`
      - [x] сериализованный JSON содержит `$type` discriminator
- [x] README с примерами:
      ```json
      [
        {"$type": "RequiredRule"},
        {"$type": "StringLengthRule", "min": 3, "max": 100}
      ]
      ```

### 3.6 Совместимость монолит/микросервисы

Stateless. Правила могут потребовать I/O только в `UniqueRule` и кастомных
правилах через Expressions — для них применяются те же принципы, что в этапе 2.

### 3.7 Definition of Done

- [x] Все правила покрыты тестами (45 unit-тестов всего).
- [x] Сериализация/десериализация работает для всех встроенных типов
      (включая `CompareRule` с enum-параметром, `EnumValueRule` с массивом).
- [x] README с примерами.
- [ ] ~~Proof of concept в существующем модуле~~ — отложен до первого
      реального потребителя (Documents), чтобы не вносить искусственный
      use case в Identity/Customer.

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
