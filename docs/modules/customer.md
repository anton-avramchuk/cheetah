# Модуль `Cheetah.Modules.Customer.*` — план реализации

> **Статус:** черновик плана, реализация не начата.
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
Customer.Domain         → DomainEvents + Specification + Contracts (abstract CustomerBase, ICustomerFactory, generic-спеки)
Customer.Infrastructure → Domain + EF + EF.PostgreSql              (abstract DbContextBase/ConfigBase, AddCustomerInfrastructure<>)
Customer.Application     → Domain + Contracts + CQRS + Events      (generic handlers, AddCustomerApplication<>)
Customer.Api            → Application + Contracts + AspNetCore      (abstract CustomerEndpointsBase<…>, ApiModuleBase)
Tests: Domain.Tests, Application.Tests
```

> **Client намеренно отсутствует** — HTTP-клиент для server-to-server создаст
> конкретная реализация (у неё будут закрытые типы DTO/Request).

> Зависимость `Domain → Contracts` нужна, потому что фабрика принимает абстрактный
> `CreateCustomerRequestBase`. Это допустимо: Contracts зависит только от Core+Shared,
> цикла не возникает.

---

## 3. Детальный план по проектам

### Фаза 0. Подготовка
- [ ] Создать дерево каталогов `src/Modules/Customer/Cheetah.Modules.Customer.{Layer}`
- [ ] Сверить версии пакетов в `Directory.Packages.props` (EF Core, Hosting.Abstractions и т.п.) — добавить отсутствующие `<PackageVersion>`
- [ ] **`Phone` ValueObject** в `src/Cheetah.Core.Domain/ValueObjects/Phone.cs` (по образцу `Email.cs`):
  - [ ] `partial class Phone : ValueObject`, `string Value`, приватный ctor, `static Create(string)` с нормализацией + regex-валидацией, `GetEqualityComponents`, `implicit operator string`
  - [ ] это правка **базового модуля** `Cheetah.Core.Domain` → обновить его `README.md`, если есть (правило pre-commit для base-модулей)

### Фаза 1. `Customer.Shared`
- [ ] `Cheetah.Modules.Customer.Shared.csproj` (ссылка: `Cheetah.Core`)
- [ ] `CheetahCustomerSharedModule.cs` — `[DependsOn(typeof(CoreModule))] class … : CrmModule`
- [ ] `CustomerConstants.cs`:
  - [ ] `ConnectionStringName = "Customer"`
  - [ ] `MaxNameLength = 256`, `MaxEmailLength = 320`, `MaxPhoneLength = 32`
  - [ ] `DefaultSchema = "customer"`, `DefaultTableName = "Customers"`
  - [ ] `DefaultRoutePrefix = "api/customers"`
- [ ] `CustomerStatus.cs` — enum `Active / Inactive / Archived`

### Фаза 2. `Customer.DomainEvents`
- [ ] `Cheetah.Modules.Customer.DomainEvents.csproj` (ссылка: `Cheetah.Core.Events`)
- [ ] `CheetahCustomerDomainEventsModule.cs` — `[DependsOn(CoreModule, CrmEventsCoreModule)]`
- [ ] `CustomerCreatedEvent.cs` — `record(Guid CustomerId, string DisplayName) : EventBase`
- [ ] `CustomerRenamedEvent.cs` — `record(Guid CustomerId, string DisplayName)`
- [ ] `CustomerContactsChangedEvent.cs` — `record(Guid CustomerId, string? Email, string? Phone)`
- [ ] `CustomerArchivedEvent.cs` — `record(Guid CustomerId)`

> События **конкретны** (id уже `Guid`) — наследник переиспользует их как есть и при
> необходимости публикует свои.

### Фаза 3. `Customer.Contracts` (абстрактные)
- [ ] `Cheetah.Modules.Customer.Contracts.csproj` (ссылки: `Core`, `Customer.Shared`)
- [ ] `CheetahCustomerContractsModule.cs` — `[DependsOn(CoreModule, CheetahCustomerSharedModule)]`
- [ ] `CustomerDtoBase.cs` — `public abstract record CustomerDtoBase` с core-полями
      (`Id, DisplayName, Email, Phone, Status, CreatedAt`); `Email`/`Phone` — `string?`
      (граница API, VO не протекают наружу)
- [ ] `CreateCustomerRequestBase.cs` — `public abstract record CreateCustomerRequestBase`
      (`DisplayName, Email, Phone` — `string?`)
- [ ] `UpdateCustomerRequestBase.cs` — `public abstract record UpdateCustomerRequestBase`
      (`DisplayName, Email, Phone` — `string?`)

> Абстрактные record нельзя инстанцировать/забиндить — наследник объявляет
> `public sealed record CustomerDto : CustomerDtoBase` и т.д. Generic-код модуля
> закрывается этими конкретными типами.

### Фаза 4. `Customer.Domain`
- [ ] `Cheetah.Modules.Customer.Domain.csproj` (ссылки: `Core`, `Core.Domain`,
      `Core.Specification`, `Customer.Shared`, `Customer.DomainEvents`, `Customer.Contracts`,
      генератор модулей как Analyzer)
- [ ] `CheetahCustomerDomainModule.cs` — `[DependsOn(CoreModule, CrmDomainModule, CrmSpecificationModule, SharedModule, DomainEventsModule, ContractsModule)]`
- [ ] `Entities/CustomerBase.cs`:
  - [ ] `abstract class CustomerBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity, IRemovedAtEntity`
  - [ ] свойства `DisplayName : string`, `Email : Email?`, `Phone : Phone?`, `Status` — все `private set`; audit-поля
  - [ ] `protected CustomerBase() {}` для EF
  - [ ] `protected void InitializeCore(Guid id, string displayName, string? email, string? phone)`
        — принимает строки, конвертирует через `Email.Create`/`Phone.Create` (null → null);
        `Status = Active` + `AddDomainEvent(new CustomerCreatedEvent(...))`
  - [ ] `public void Rename(string name)` → `CustomerRenamedEvent`
  - [ ] `public void ChangeContacts(string? email, string? phone)` — строки → VO →
        `CustomerContactsChangedEvent` (в событии — строковые значения VO)
  - [ ] `public void Archive()` → `Status = Archived` + `RemovedAt` + `CustomerArchivedEvent`
  - [ ] `protected virtual` hook'и для расширения (опционально)
- [ ] `Abstractions/ICustomerFactory.cs`:
  - [ ] `interface ICustomerFactory<TCustomer, TCreateRequest> where TCustomer : CustomerBase where TCreateRequest : CreateCustomerRequestBase`
  - [ ] метод `TCustomer Create(TCreateRequest request)`
- [ ] `Specifications/CustomerSpecifications.cs` (generic, `where TCustomer : CustomerBase`):
  - [ ] `CustomerByEmailSpecification<TCustomer>`
  - [ ] `CustomerByIdsSpecification<TCustomer>`
  - [ ] `ActiveCustomersSpecification<TCustomer>` (Status == Active)

### Фаза 5. `Customer.Infrastructure`
- [ ] `Cheetah.Modules.Customer.Infrastructure.csproj` (ссылки: `Core`, `Core.DataAccess`,
      `Core.Domain`, `Core.EntityFramework`, `Core.EntityFramework.PostgreSql`,
      `Customer.Domain`, генератор; пакеты EFCore/Relational/Hosting.Abstractions/Logging.Abstractions/EFCore.Design)
  - [ ] `<EmitCompilerGeneratedFiles>true</…>`
- [ ] `CheetahCustomerInfrastructureModule.cs` — `[DependsOn(CoreModule, CrmDataAccessModule, CrmDomainModule, CrmEntityFrameworkModule, CrmEntityFrameworkPostgreSqlModule, CheetahCustomerDomainModule)]`
      (только `RegisterServices`; конкретный DbContext НЕ регистрирует — это делает наследник)
- [ ] `Persistence/CustomerDbContextBase.cs`:
  - [ ] `abstract class CustomerDbContextBase<TContext, TCustomer> : CrmDbContext<TContext> where TContext : DbContext where TCustomer : CustomerBase`
  - [ ] `DbSet<TCustomer> Customers => Set<TCustomer>()`
  - [ ] `protected override void OnModelCreating(ModelBuilder b)` → `b.ApplyConfiguration(CreateCustomerConfiguration())`
  - [ ] `protected abstract IEntityTypeConfiguration<TCustomer> CreateCustomerConfiguration()`
- [ ] `Persistence/Configurations/CustomerConfigurationBase.cs`:
  - [ ] `abstract class CustomerConfigurationBase<TCustomer> : IEntityTypeConfiguration<TCustomer> where TCustomer : CustomerBase`
  - [ ] `virtual Configure(...)`: `ToTable(TableName, Schema)`, `Ignore(DomainEvents)`,
        `HasKey(Id)`, длины из `CustomerConstants`, индекс по `Email`, вызов `ConfigureCustom`
  - [ ] **VO-конвертеры**: `Property(x => x.Email).HasConversion(e => e.Value, v => Email.Create(v)).HasMaxLength(MaxEmailLength)`
        и аналогично `Phone` (конвертер не вызывается для `null` → nullable-колонки работают как есть)
  - [ ] `protected virtual string TableName/Schema` (из констант)
  - [ ] `protected virtual void ConfigureCustom(EntityTypeBuilder<TCustomer> b) {}` — hook
- [ ] `Extensions/CustomerInfrastructureServiceCollectionExtensions.cs`:
  - [ ] `AddCustomerInfrastructure<TContext, TCustomer>(this IServiceCollection)`
        `where TContext : CustomerDbContextBase<TContext, TCustomer> where TCustomer : CustomerBase`
  - [ ] внутри: `AddApplicationDbContext<TContext>()`, `AddScoped<TContext>()`,
        `AddDatabaseMigrator<TContext>()`, `Configure<CrmDbContextOptions>(o => o.UseNpgsql<TContext>())`,
        `AddScoped<IRepository<TCustomer,Guid>, EfRepository<TContext,TCustomer,Guid>>()`

> Миграций и `IDesignTimeDbContextFactory` в модуле **нет** — наследник создаёт свой
> конкретный `DbContext` и генерирует миграции у себя.

### Фаза 6. `Customer.Application` (generic CQRS)
- [ ] `Cheetah.Modules.Customer.Application.csproj` (ссылки: `Core`, `Core.CQRS`,
      `Core.DataAccess`, `Core.Events`, `Customer.Domain`, `Customer.Contracts`, генератор)
- [ ] `CheetahCustomerApplicationModule.cs` — `[DependsOn(CoreModule, CrmCQRSCoreModule, CrmDataAccessModule, CrmEventsCoreModule, DomainModule, ContractsModule, DomainEventsModule)]`
- [ ] `Exceptions/CustomerValidationException.cs`
- [ ] `Customers/CreateCustomerCommand.cs`:
  - [ ] `record CreateCustomerCommand<TCreateRequest>(TCreateRequest Request) : ICommand<Guid> where TCreateRequest : CreateCustomerRequestBase`
  - [ ] `CreateCustomerCommandHandler<TCustomer, TCreateRequest> : ICommandHandler<CreateCustomerCommand<TCreateRequest>, Guid>`
        — inject `ICustomerFactory<,>`, `IRepository<TCustomer,Guid>`, `IEventBus`;
        `factory.Create → repo.Add → SaveChangesAsync → publish DomainEvents → ClearDomainEvents`
- [ ] `Customers/UpdateCustomerCommand.cs`:
  - [ ] `record UpdateCustomerCommand<TUpdateRequest>(Guid Id, TUpdateRequest Request) : ICommand`
  - [ ] handler `…<TCustomer, TUpdateRequest>`: `GetById → Rename/ChangeContacts → Save → publish`
- [ ] `Customers/ArchiveCustomerCommand.cs`:
  - [ ] `record ArchiveCustomerCommand(Guid Id) : ICommand`
  - [ ] handler `…<TCustomer>`: `GetById → Archive() → Save → publish`
- [ ] `Customers/GetCustomerByIdQuery.cs`:
  - [ ] `record GetCustomerByIdQuery<TDto>(Guid Id) : IQuery<TDto?> where TDto : CustomerDtoBase`
  - [ ] handler `…<TCustomer, TDto>`: `AsNoTracking → IObjectMapper.Map<TDto>`
- [ ] `Customers/ListCustomersQuery.cs`:
  - [ ] `record ListCustomersQuery<TDto>(bool ActiveOnly = false) : IQuery<IReadOnlyList<TDto>>`
  - [ ] handler `…<TCustomer, TDto>`: спека + проекция через `IObjectMapper`
- [ ] `Extensions/CustomerApplicationServiceCollectionExtensions.cs`:
  - [ ] `AddCustomerApplication<TCustomer, TCreateRequest, TUpdateRequest, TDto, TFactory>(this IServiceCollection)`
  - [ ] регистрация фабрики и всех закрытых handler'ов (`ICommandHandler<…>`, `IQueryHandler<…>`)

> Маппинг `TCustomer → TDto` — через `IObjectMapper` (Mapster). Наследник при желании
> добавляет правила маппинга для своих доп. полей.

### Фаза 7. `Customer.Api` (абстрактные эндпоинты)
- [ ] `Cheetah.Modules.Customer.Api.csproj` (ссылки: `Core`, `Core.CQRS`, `AspNetCore`,
      `Customer.Application`, `Customer.Contracts`)
- [ ] `Endpoints/CustomerEndpointsBase.cs`:
  - [ ] `abstract class CustomerEndpointsBase<TCustomer, TCreateRequest, TUpdateRequest, TDto>`
        с ограничениями на base-типы Contracts/Domain
  - [ ] `public void Map(IEndpointRouteBuilder routes)` — `POST/GET{id}/GET/PUT{id}/DELETE{id}`
        на `RoutePrefix`, диспетчеризация в `IDispatcher` с закрытыми generic-командами/запросами
  - [ ] `protected virtual string RoutePrefix => CustomerConstants.DefaultRoutePrefix`
  - [ ] обработка `CustomerValidationException → BadRequest` (как в `CheetahTagsApiModule`)
  - [ ] хендлеры эндпоинтов `protected virtual` — чтобы наследник мог переопределить отдельный маршрут
- [ ] `CheetahCustomerApiModuleBase.cs`:
  - [ ] `abstract class CheetahCustomerApiModuleBase<…> : CrmModule`
  - [ ] `OnApplicationInitialization` → `new {Endpoints}().Map(context.GetRouteBuilder())`
  - [ ] наследник ставит `[DependsOn]` на свои Application/Contracts и закрывает generic-параметры

### Фаза 8. Тесты
- [ ] `Customer.Domain.Tests` — тестовый наследник `TestCustomer`: инварианты,
      доменные события (`Created/Renamed/ContactsChanged/Archived`), переходы статуса
- [ ] `Customer.Application.Tests` — generic-handler'ы на `TestCustomer`/`TestCreateRequest`/`TestDto`
      с fake `IRepository`/`ICustomerFactory`/`IEventBus`/`IObjectMapper`

### Фаза 9. Интеграция в решение
- [ ] `dotnet sln Cheetah.slnx add` всех проектов в папку `/Modules/Customer/`
- [ ] добавить новую `<Folder Name="/Modules/Customer/">` в `Cheetah.slnx`
- [ ] `dotnet build Cheetah.slnx` — сборка зелёная
- [ ] `dotnet test` по двум тестовым проектам — зелёные

### Фаза 10. Документация модуля
- [ ] `src/Modules/Customer/README.md` — назначение, граф зависимостей, **пример наследования**
      (см. §4), список extension-методов, ограничение «миграции у наследника»
- [ ] удалить возможные `nul`-файлы перед коммитом

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
- [ ] `DisplayName` через `Email`-маппинг в DTO: при проекции `TCustomer → TDto` через Mapster нужно правило `Email? → string?` (`e => e == null ? null : e.Value`) — наследник регистрирует, либо добавить глобальный конвертер в `CrmMapsterModule`.
- [ ] Soft-delete: `Archive()` ставит `Status=Archived` + `RemovedAt`. Решить, нужен ли глобальный query-filter `RemovedAt == null` в `CustomerConfigurationBase` (по умолчанию — нет, чтобы не скрывать архив).
- [ ] Нужен ли отдельный `RestoreCustomerCommand` (разархивация) — пока вне scope.
- [ ] Подписки на события Identity (как в Tags) в Customer **не предусмотрены** — добавить только если появится реплика пользователей.

---

## 6. Чек-лист прогресса (сводка по фазам)

- [ ] Фаза 0 — Подготовка
- [ ] Фаза 1 — Shared
- [ ] Фаза 2 — DomainEvents
- [ ] Фаза 3 — Contracts (абстрактные)
- [ ] Фаза 4 — Domain
- [ ] Фаза 5 — Infrastructure
- [ ] Фаза 6 — Application
- [ ] Фаза 7 — Api (абстрактные эндпоинты)
- [ ] Фаза 8 — Тесты
- [ ] Фаза 9 — Интеграция в решение
- [ ] Фаза 10 — Документация
