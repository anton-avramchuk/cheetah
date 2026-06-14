# Модуль `Cheetah.Modules.Customer.*` — план реализации

> **Статус:** реализация завершена — все сборки в `Cheetah.slnx`, решение собирается, тесты зелёные (Domain 10 / Application 7), README модуля добавлен.
> **Тип модуля:** абстрактный шаблон-каркас (base/template), а не готовый микросервис.
> Отмечай прогресс галочками `- [x]` по мере выполнения.

---

## 1. Концепция

`Customer` поставляет **только абстрактные базовые типы и generic-хелперы**. Нижестоящий
модуль/приложение наследует конкретные типы, генерирует миграции у себя и получает рабочий
CRUD «из коробки», дописав минимум кода.

Зафиксированные решения:

- **Полный generic-стек** — абстрактны Domain, DbContext, конфигурация, **Contracts** и **эндпоинты**;
  Application/Api переиспользуемы через generic-handlers и generic-mapping.
- **Чистые абстракции без миграций** — конкретный `DbContext`, конфигурацию сущностей,
  миграции и design-time фабрику создаёт наследник. В модуле миграций **нет**.
- **Фиксированный `Guid`** — `CustomerBase : AggregateRoot<Guid>`.
- **Client НЕ делаем** — HTTP-клиент создаст наследник на этапе конкретной реализации.
- **Контакты — ValueObject**: `Email` (есть в `Cheetah.Core.Domain`) и `Phone` (нужно
  создать в `Core.Domain` рядом с `Email`). В **Domain-сущности** поля — VO (`Email?`/`Phone?`);
  в **Contracts** (граница API) остаются `string?`; конвертация string↔VO — в фабрике/мутаторах
  и через EF value converter.

### Отличия от эталона `Tags`

| Аспект | Tags (эталон) | Customer (шаблон) |
|---|---|---|
| Domain-сущность | конкретный `Tag` | **abstract `CustomerBase`** |
| DTO/Request (Contracts) | конкретные record | **abstract `…Base` (record/class)** |
| DbContext | конкретный `TagsDbContext` | **abstract `CustomerDbContextBase<TContext,TCustomer>`** |
| Конфигурация | конкретный `TagConfiguration` | **abstract `CustomerConfigurationBase<TCustomer>`** |
| Команды/Запросы | конкретные | **generic над request/dto** |
| Handlers | конкретные с `[Export]` | **generic `…Handler<…>` + ручная регистрация в extension** |
| Endpoints | inline в `OnApplicationInitialization` | **abstract generic-база `CustomerEndpointsBase<…>`** |
| Миграции / design-time factory | есть | **нет (создаёт наследник)** |
| Регистрация DI | `AddApplicationDbContext<TagsDbContext>()` | **`AddCustomerInfrastructure<TContext,TCustomer>()` extension** |

### Архитектурные приёмы, решающие «абстрактность»

1. **Нельзя `new` абстрактную сущность в generic-handler** → вводим фабрику
   `ICustomerFactory<TCustomer,TCreateRequest>`, реализуемую наследником.
2. **`[Export]` source-gen работает только по закрытым типам** → открытые generic
   handlers/репозитории регистрируем вручную внутри extension-методов
   (`AddCustomerInfrastructure<>`, `AddCustomerApplication<>`), а не атрибутом.
3. **Абстрактный DbContext нельзя мигрировать** → миграции и `IDesignTimeDbContextFactory`
   принадлежат наследнику; базовый `OnModelCreating` применяет конфигурацию через hook.
4. **Абстрактные Contracts нельзя забиндить/инстанцировать** → endpoints generic над
   конкретными `TCreateRequest/TUpdateRequest/TDto : …Base`; маппинг `TCustomer → TDto`
   через `IObjectMapper` (Mapster), маппинг `TRequest → TCustomer` через фабрику.

---

## 2. Состав сборок и граф зависимостей

Путь: `src/Modules/Customer/`. 7 сборок + тесты (Client опущен — его сделает наследник).

```
Customer.DomainEvents   → Core.Events                              (конкретные record-события, Guid id)
Customer.Shared         → Core                                     (константы, enum CustomerStatus)
Customer.Contracts      → Core + Shared                            (ABSTRACT базовые DTO/Request)
Customer.Domain         → DomainEvents + Specification             (abstract CustomerBase, generic-спеки)
Customer.Infrastructure → Domain + EF + EF.PostgreSql              (abstract DbContextBase/ConfigBase, AddCustomerInfrastructure<>)
Customer.Application     → Domain + Contracts + CQRS + Events      (generic handlers, AddCustomerApplication<>)
Customer.Api            → Application + Contracts + AspNetCore      (abstract CustomerEndpointsBase<…>, ApiModuleBase)
Tests: Domain.Tests, Application.Tests
```

> **Client намеренно отсутствует** — HTTP-клиент для server-to-server создаст
> конкретная реализация (у неё будут закрытые типы DTO/Request).

> `ICustomerFactory` вынесен в **Application** (маппинг request→entity — прикладная забота),
> поэтому Domain не зависит от Contracts и остаётся чистым, как у `Tags`.

---

## 3. Детальный план по проектам

### Фаза 0. Подготовка
- [x] Создать дерево каталогов `src/Modules/Customer/Cheetah.Modules.Customer.{Layer}`
- [x] Сверить версии пакетов в `Directory.Packages.props` (EF Core, Hosting.Abstractions и т.п.) — добавить отсутствующие `<PackageVersion>`
- [x] **`Phone` ValueObject** в `src/Cheetah.Core.Domain/ValueObjects/Phone.cs` (по образцу `Email.cs`):
  - [x] `partial class Phone : ValueObject`, `string Value`, приватный ctor, `static Create(string)` с нормализацией + regex-валидацией, `GetEqualityComponents`, `implicit operator string`
  - [x] это правка **базового модуля** `Cheetah.Core.Domain` → обновить его `README.md`, если есть (правило pre-commit для base-модулей)

### Фаза 1. `Customer.Shared`
- [x] `Cheetah.Modules.Customer.Shared.csproj` (ссылка: `Cheetah.Core`)
- [x] `CheetahCustomerSharedModule.cs` — `[DependsOn(typeof(CoreModule))] class … : CrmModule`
- [x] `CustomerConstants.cs`:
  - [x] `ConnectionStringName = "Customer"`
  - [x] `MaxNameLength = 256`, `MaxEmailLength = 320`, `MaxPhoneLength = 32`
  - [x] `DefaultSchema = "customer"`, `DefaultTableName = "Customers"`
  - [x] `DefaultRoutePrefix = "api/customers"`
- [x] `CustomerStatus.cs` — enum `Active / Inactive / Archived`

### Фаза 2. `Customer.DomainEvents`
- [x] `Cheetah.Modules.Customer.DomainEvents.csproj` (ссылка: `Cheetah.Core.Events`)
- [x] `CheetahCustomerDomainEventsModule.cs` — `[DependsOn(CoreModule, CrmEventsCoreModule)]`
- [x] `CustomerCreatedEvent.cs` — `record(Guid CustomerId, string DisplayName) : EventBase`
- [x] `CustomerRenamedEvent.cs` — `record(Guid CustomerId, string DisplayName)`
- [x] `CustomerContactsChangedEvent.cs` — `record(Guid CustomerId, string? Email, string? Phone)`
- [x] `CustomerArchivedEvent.cs` — `record(Guid CustomerId)`

> События **конкретны** (id уже `Guid`) — наследник переиспользует их как есть и при
> необходимости публикует свои.

### Фаза 3. `Customer.Contracts` (абстрактные)
- [x] `Cheetah.Modules.Customer.Contracts.csproj` (ссылки: `Core`, `Customer.Shared`)
- [x] `CheetahCustomerContractsModule.cs` — `[DependsOn(CoreModule, CheetahCustomerSharedModule)]`
- [x] `CustomerDtoBase.cs` — `public abstract record CustomerDtoBase` с core-полями
      (`Id, DisplayName, Email, Phone, Status, CreatedAt`); `Email`/`Phone` — `string?`
      (граница API, VO не протекают наружу)
- [x] `CreateCustomerRequestBase.cs` — `public abstract record CreateCustomerRequestBase`
      (`DisplayName, Email, Phone` — `string?`)
- [x] `UpdateCustomerRequestBase.cs` — `public abstract record UpdateCustomerRequestBase`
      (`DisplayName, Email, Phone` — `string?`)

> Абстрактные record нельзя инстанцировать/забиндить — наследник объявляет
> `public sealed record CustomerDto : CustomerDtoBase` и т.д. Generic-код модуля
> закрывается этими конкретными типами.

### Фаза 4. `Customer.Domain`
- [x] `Cheetah.Modules.Customer.Domain.csproj` (ссылки: `Core`, `Core.Domain`,
      `Core.Specification`, `Customer.Shared`, `Customer.DomainEvents`,
      генератор модулей как Analyzer)
- [x] `CheetahCustomerDomainModule.cs` — `[DependsOn(CoreModule, CrmDomainModule, CrmSpecificationModule, SharedModule, DomainEventsModule)]`
- [x] `Entities/CustomerBase.cs`:
  - [x] `abstract class CustomerBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity, IRemovedAtEntity`
  - [x] свойства `DisplayName : string`, `Email : Email?`, `Phone : Phone?`, `Status` — все `private set`; audit-поля
  - [x] `protected CustomerBase() {}` для EF
  - [x] `protected void InitializeCore(Guid id, string displayName, string? email, string? phone)`
        — принимает строки, конвертирует через `Email.Create`/`Phone.Create` (null → null);
        `Status = Active` + `AddDomainEvent(new CustomerCreatedEvent(...))`
  - [x] `public void Rename(string name)` → `CustomerRenamedEvent`
  - [x] `public void ChangeContacts(string? email, string? phone)` — строки → VO →
        `CustomerContactsChangedEvent` (в событии — строковые значения VO)
  - [x] `public void Archive()` → `Status = Archived` + `RemovedAt` + `CustomerArchivedEvent`
  - [x] `protected virtual` hook'и для расширения (опционально)
- [x] `Specifications/CustomerSpecifications.cs` (generic, `where TCustomer : CustomerBase`):
  - [x] `CustomerByEmailSpecification<TCustomer>`
  - [x] `CustomerByIdsSpecification<TCustomer>`
  - [x] `ActiveCustomersSpecification<TCustomer>` (Status == Active)

### Фаза 5. `Customer.Infrastructure`
- [x] `Cheetah.Modules.Customer.Infrastructure.csproj` (ссылки: `Core`, `Core.DataAccess`,
      `Core.Domain`, `Core.EntityFramework`, `Core.EntityFramework.PostgreSql`,
      `Customer.Domain`, генератор; пакеты EFCore/Relational/Hosting.Abstractions/Logging.Abstractions/EFCore.Design)
  - [x] `<EmitCompilerGeneratedFiles>true</…>`
- [x] `CheetahCustomerInfrastructureModule.cs` — `[DependsOn(CoreModule, CrmDataAccessModule, CrmDomainModule, CrmEntityFrameworkModule, CrmEntityFrameworkPostgreSqlModule, CheetahCustomerDomainModule)]`
      (только `RegisterServices`; конкретный DbContext НЕ регистрирует — это делает наследник)
- [x] `Persistence/CustomerDbContextBase.cs`:
  - [x] `abstract class CustomerDbContextBase<TContext, TCustomer> : CrmDbContext<TContext> where TContext : DbContext where TCustomer : CustomerBase`
  - [x] `DbSet<TCustomer> Customers => Set<TCustomer>()`
  - [x] `protected override void OnModelCreating(ModelBuilder b)` → `b.ApplyConfiguration(CreateCustomerConfiguration())`
  - [x] `protected abstract IEntityTypeConfiguration<TCustomer> CreateCustomerConfiguration()`
- [x] `Persistence/Configurations/CustomerConfigurationBase.cs`:
  - [x] `abstract class CustomerConfigurationBase<TCustomer> : IEntityTypeConfiguration<TCustomer> where TCustomer : CustomerBase`
  - [x] `virtual Configure(...)`: `ToTable(TableName, Schema)`, `Ignore(DomainEvents)`,
        `HasKey(Id)`, длины из `CustomerConstants`, индекс по `Email`, вызов `ConfigureCustom`
  - [x] **VO-конвертеры**: `Property(x => x.Email).HasConversion(e => e.Value, v => Email.Create(v)).HasMaxLength(MaxEmailLength)`
        и аналогично `Phone` (конвертер не вызывается для `null` → nullable-колонки работают как есть)
  - [x] `protected virtual string TableName/Schema` (из констант)
  - [x] `protected virtual void ConfigureCustom(EntityTypeBuilder<TCustomer> b) {}` — hook
- [x] `Extensions/CustomerInfrastructureServiceCollectionExtensions.cs`:
  - [x] `AddCustomerInfrastructure<TContext, TCustomer>(this IServiceCollection)`
        `where TContext : CustomerDbContextBase<TContext, TCustomer> where TCustomer : CustomerBase`
  - [x] внутри: `AddApplicationDbContext<TContext>()`, `AddScoped<TContext>()`,
        `AddDatabaseMigrator<TContext>()`, `Configure<CrmDbContextOptions>(o => o.UseNpgsql<TContext>())`,
        `AddScoped<IRepository<TCustomer,Guid>, EfRepository<TContext,TCustomer,Guid>>()`

> Миграций и `IDesignTimeDbContextFactory` в модуле **нет** — наследник создаёт свой
> конкретный `DbContext` и генерирует миграции у себя.

### Фаза 6. `Customer.Application` (generic CQRS)
- [x] `Cheetah.Modules.Customer.Application.csproj` (ссылки: `Core`, `Core.CQRS`,
      `Core.DataAccess`, `Core.Events`, `Customer.Domain`, `Customer.Contracts`, генератор)
- [x] `CheetahCustomerApplicationModule.cs` — `[DependsOn(CoreModule, CrmCQRSCoreModule, CrmDataAccessModule, CrmEventsCoreModule, DomainModule, ContractsModule, DomainEventsModule)]`
- [x] `Abstractions/ICustomerFactory.cs` — `interface ICustomerFactory<TCustomer, TCreateRequest> where TCustomer : CustomerBase where TCreateRequest : CreateCustomerRequestBase` с методом `TCustomer Create(TCreateRequest request)`
- [x] `Exceptions/CustomerValidationException.cs`
- [x] `Customers/CreateCustomerCommand.cs`:
  - [x] `record CreateCustomerCommand<TCreateRequest>(TCreateRequest Request) : ICommand<Guid> where TCreateRequest : CreateCustomerRequestBase`
  - [x] `CreateCustomerCommandHandler<TCustomer, TCreateRequest> : ICommandHandler<CreateCustomerCommand<TCreateRequest>, Guid>`
        — inject `ICustomerFactory<,>`, `IRepository<TCustomer,Guid>`, `IEventBus`;
        `factory.Create → repo.Add → SaveChangesAsync → publish DomainEvents → ClearDomainEvents`
- [x] `Customers/UpdateCustomerCommand.cs`:
  - [x] `record UpdateCustomerCommand<TUpdateRequest>(Guid Id, TUpdateRequest Request) : ICommand`
  - [x] handler `…<TCustomer, TUpdateRequest>`: `GetById → Rename/ChangeContacts → Save → publish`
- [x] `Customers/ArchiveCustomerCommand.cs`:
  - [x] `record ArchiveCustomerCommand(Guid Id) : ICommand`
  - [x] handler `…<TCustomer>`: `GetById → Archive() → Save → publish`
- [x] `Customers/GetCustomerByIdQuery.cs`:
  - [x] `record GetCustomerByIdQuery<TDto>(Guid Id) : IQuery<TDto?> where TDto : CustomerDtoBase`
  - [x] handler `…<TCustomer, TDto>`: `AsNoTracking → IObjectMapper.Map<TDto>`
- [x] `Customers/ListCustomersQuery.cs`:
  - [x] `record ListCustomersQuery<TDto>(bool ActiveOnly = false) : IQuery<IReadOnlyList<TDto>>`
  - [x] handler `…<TCustomer, TDto>`: спека + проекция через `IObjectMapper`
- [x] `Extensions/CustomerApplicationServiceCollectionExtensions.cs`:
  - [x] `AddCustomerApplication<TCustomer, TCreateRequest, TUpdateRequest, TDto, TFactory>(this IServiceCollection)`
  - [x] регистрация фабрики и всех закрытых handler'ов (`ICommandHandler<…>`, `IQueryHandler<…>`)

> Маппинг `TCustomer → TDto` — через `IObjectMapper` (Mapster). Наследник при желании
> добавляет правила маппинга для своих доп. полей.

### Фаза 7. `Customer.Api` (абстрактные эндпоинты)
- [x] `Cheetah.Modules.Customer.Api.csproj` (ссылки: `Core`, `Core.CQRS`, `AspNetCore`,
      `Customer.Application`, `Customer.Contracts`)
- [x] `Endpoints/CustomerEndpointsBase.cs`:
  - [x] `abstract class CustomerEndpointsBase<TCustomer, TCreateRequest, TUpdateRequest, TDto>`
        с ограничениями на base-типы Contracts/Domain
  - [x] `public void Map(IEndpointRouteBuilder routes)` — `POST/GET{id}/GET/PUT{id}/DELETE{id}`
        на `RoutePrefix`, диспетчеризация в `IDispatcher` с закрытыми generic-командами/запросами
  - [x] `protected virtual string RoutePrefix => CustomerConstants.DefaultRoutePrefix`
  - [x] обработка `CustomerValidationException → BadRequest` (как в `CheetahTagsApiModule`)
  - [x] хендлеры эндпоинтов `protected virtual` — чтобы наследник мог переопределить отдельный маршрут
- [x] `CheetahCustomerApiModuleBase.cs`:
  - [x] `abstract class CheetahCustomerApiModuleBase<…> : CrmModule`
  - [x] `OnApplicationInitialization` → `new {Endpoints}().Map(context.GetRouteBuilder())`
  - [x] наследник ставит `[DependsOn]` на свои Application/Contracts и закрывает generic-параметры

### Фаза 8. Тесты
- [x] `Customer.Domain.Tests` — тестовый наследник `TestCustomer`: инварианты,
      доменные события (`Created/Renamed/ContactsChanged/Archived`), переходы статуса
- [x] `Customer.Application.Tests` — generic-handler'ы на `TestCustomer`/`TestCreateRequest`/`TestDto`
      с fake `IRepository`/`ICustomerFactory`/`IEventBus`/`IObjectMapper`

### Фаза 9. Интеграция в решение
- [x] `dotnet sln Cheetah.slnx add` всех проектов в папку `/Modules/Customer/`
- [x] добавить новую `<Folder Name="/Modules/Customer/">` в `Cheetah.slnx`
- [x] `dotnet build Cheetah.slnx` — сборка зелёная
- [x] `dotnet test` по двум тестовым проектам — зелёные

### Фаза 10. Документация модуля
- [x] `src/Modules/Customer/README.md` — назначение, граф зависимостей, **пример наследования**
      (см. §4), список extension-методов, ограничение «миграции у наследника»
- [x] удалить возможные `nul`-файлы перед коммитом

---

## 4. Сторона наследника (пример «быстрой реализации»)

Минимальный набор, который пишет потребитель шаблона:

```csharp
// 1. Конкретная сущность
public sealed class Customer : CustomerBase
{
    public string? Inn { get; private set; }            // доп. поле наследника
    private Customer() {}
    public static Customer Create(CreateCustomerRequest r)
    {
        var c = new Customer();
        c.InitializeCore(Guid.NewGuid(), r.DisplayName, r.Email, r.Phone);
        c.Inn = r.Inn;
        return c;
    }
}

// 2. Конкретные Contracts
public sealed record CustomerDto : CustomerDtoBase { public string? Inn { get; init; } }
public sealed record CreateCustomerRequest : CreateCustomerRequestBase { public string? Inn { get; init; } }
public sealed record UpdateCustomerRequest : UpdateCustomerRequestBase;

// 3. Фабрика
public sealed class CustomerFactory : ICustomerFactory<Customer, CreateCustomerRequest>
{ public Customer Create(CreateCustomerRequest r) => Customer.Create(r); }

// 4. Конфигурация + DbContext
public sealed class CustomerConfiguration : CustomerConfigurationBase<Customer>
{ protected override void ConfigureCustom(EntityTypeBuilder<Customer> b) => b.Property(x => x.Inn).HasMaxLength(12); }

public sealed class AppCustomerDbContext(DbContextOptions<AppCustomerDbContext> o)
    : CustomerDbContextBase<AppCustomerDbContext, Customer>(o)
{ protected override IEntityTypeConfiguration<Customer> CreateCustomerConfiguration() => new CustomerConfiguration(); }

// 5. Регистрация (в модулях наследника)
services.AddCustomerInfrastructure<AppCustomerDbContext, Customer>();
services.AddCustomerApplication<Customer, CreateCustomerRequest, UpdateCustomerRequest, CustomerDto, CustomerFactory>();

// 6. Api-модуль наследника
public sealed class AppCustomerEndpoints
    : CustomerEndpointsBase<Customer, CreateCustomerRequest, UpdateCustomerRequest, CustomerDto> {}

// 7. dotnet ef migrations add Initial  (+ IDesignTimeDbContextFactory) — в проекте наследника
```

Итого ~6–7 классов + миграция → рабочий CRUD-модуль Customer.

---

## 5. Открытые вопросы / заметки

- [x] `Email`/`Phone` — ValueObject в Domain (`Email` из `Core.Domain`, `Phone` создаём там же), строки в Contracts. **Решено.**
- [x] Проекция `TCustomer → TDto`: вместо Mapster введён `ICustomerProjector<TCustomer, TDto>` (реализует наследник) — VO `Email?/Phone? → string?` без скрытой конфигурации маппера. **Решено.**
- [ ] Soft-delete: `Archive()` ставит `Status=Archived` + `RemovedAt`. Решить, нужен ли глобальный query-filter `RemovedAt == null` в `CustomerConfigurationBase` (по умолчанию — нет, чтобы не скрывать архив).
- [ ] Нужен ли отдельный `RestoreCustomerCommand` (разархивация) — пока вне scope.
- [ ] Подписки на события Identity (как в Tags) в Customer **не предусмотрены** — добавить только если появится реплика пользователей.

---

## 6. Чек-лист прогресса (сводка по фазам)

- [x] Фаза 0 — Подготовка
- [x] Фаза 1 — Shared
- [x] Фаза 2 — DomainEvents
- [x] Фаза 3 — Contracts (абстрактные)
- [x] Фаза 4 — Domain
- [x] Фаза 5 — Infrastructure
- [x] Фаза 6 — Application
- [x] Фаза 7 — Api (абстрактные эндпоинты)
- [x] Фаза 8 — Тесты
- [x] Фаза 9 — Интеграция в решение
- [x] Фаза 10 — Документация

---

## 7. Расширение после базового плана: контакты + ответственный

Добавлено сверх исходного плана (по запросу — «сотрудники клиента»):

### A. Ответственный со стороны нашей компании
- [x] `OwnerId : Guid?` на `CustomerBase` (ссылка на пользователя Identity по Id, без FK)
- [x] мутатор `AssignOwner(Guid?)` + событие `CustomerOwnerChangedEvent` (только при изменении)
- [x] `OwnerId` в `CustomerDtoBase`/`Create`/`UpdateCustomerRequestBase`; вызов `AssignOwner` в update-handler
- [x] индекс по `OwnerId` в `CustomerConfigurationBase`; `CustomersByOwnerSpecification<>`

### B. Контактные лица клиента (отдельный абстрактный агрегат `ContactBase`)
- [x] DomainEvents: `ContactAddedEvent`, `ContactRenamedEvent`, `ContactContactsChangedEvent`, `ContactRemovedEvent`
- [x] Shared: длины `MaxFullNameLength`/`MaxPositionLength`, имя таблицы и суффикс маршрута контактов
- [x] Contracts: `ContactDtoBase`, `Create/UpdateContactRequestBase` (abstract record)
- [x] Domain: `ContactBase : AggregateRoot<Guid>` (`CustomerId`, `FullName`, `Position?`, `Email?`, `Phone?`; `Rename/ChangePosition/ChangeContacts/Remove`); спеки `ContactsByCustomer`/`ContactByEmail`
- [x] Infrastructure: `CustomerDbContextBase<TContext, TCustomer, TContact>` (оба агрегата в одной БД), `ContactConfigurationBase<TContact>`, регистрация `IRepository<TContact,Guid>` в `AddCustomerInfrastructure<,,>`
- [x] Application: generic CQRS `Add/Update/Remove` + `GetContactById/ListContactsByCustomer`, `IContactFactory`/`IContactProjector`, extension `AddCustomerContacts<…>`
- [x] Api: `ContactEndpointsBase<…>` (вложенные маршруты `api/customers/{customerId}/contacts`), `CheetahCustomerContactsApiModuleBase<…>`
- [x] Тесты: Domain (`ContactBaseTests`) + Application (`ContactCommandHandlerTests`/`ContactQueryHandlerTests`) — всего Domain 21 / Application 15, зелёные

> **Breaking** относительно §3-плана: `CustomerDbContextBase` и `AddCustomerInfrastructure`
> получили доп. type-параметр `TContact`. Это новый, ещё не выпущенный модуль без потребителей —
> сигнатуры эволюционированы, README обновлён.
