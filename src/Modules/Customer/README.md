# Cheetah.Modules.Customer.* — абстрактный шаблон-модуль

> **Тип:** base/template (каркас), а не готовый микросервис.
> Модуль поставляет **только абстрактные базовые типы и generic-хелперы**. Конкретное
> приложение наследует типы, генерирует миграции у себя и получает рабочий CRUD «из коробки»,
> дописав ~6–7 классов.

## Назначение

`Customer` фиксирует структуру и поведение клиента (DDD-агрегат, CQRS-handlers, CRUD-эндпоинты),
но не привязывается к конкретной схеме данных. Модуль покрывает два агрегата:

- **`CustomerBase`** — сам клиент;
- **`ContactBase`** — контактное лицо клиента (отдельный агрегат со ссылкой `CustomerId`, не child-entity).

Наследник:

- объявляет `sealed class Customer : CustomerBase` и `sealed class Contact : ContactBase` со своими доп. полями;
- объявляет конкретные `record`-ы Contracts (`CustomerDto`/`ContactDto`, `Create/Update…Request`);
- поставляет фабрики (`ICustomerFactory`/`IContactFactory`) и проекторы (`ICustomerProjector`/`IContactProjector`);
- создаёт свой `DbContext` (наследник `CustomerDbContextBase`, хостит обе таблицы) и **генерирует миграции у себя**.

Зафиксированные решения: `Guid`-идентификатор; контакты — value objects `Email`/`Phone`
(в `Cheetah.Core.Domain`), на границе API — строки; Client намеренно отсутствует (его создаёт
наследник, у которого есть закрытые типы DTO/Request).

**Сотрудники клиента.** «Контактные лица клиента» (B2B) — это агрегат `Contact`. «Наш ответственный»
за клиентом — поле `OwnerId : Guid?` на `CustomerBase` (ссылка на пользователя Identity по Id,
без FK через границу модуля), мутатор `AssignOwner`, событие `CustomerOwnerChangedEvent`.

## Состав сборок и граф зависимостей

```
Customer.DomainEvents   → Core.Events                         (конкретные record-события, Guid id)
Customer.Shared         → Core                                (константы, enum CustomerStatus)
Customer.Contracts      → Core + Shared                       (ABSTRACT базовые DTO/Request)
Customer.Domain         → DomainEvents + Specification        (abstract CustomerBase, generic-спеки)
Customer.Infrastructure → Domain + EF + EF.PostgreSql         (abstract DbContextBase/ConfigBase, AddCustomerInfrastructure<>)
Customer.Application     → Domain + Contracts + CQRS + Events (generic handlers, AddCustomerApplication<>)
Customer.Api            → Application + Contracts + AspNetCore (abstract CustomerEndpointsBase<>, ApiModuleBase)
Tests: Domain.Tests, Application.Tests
```

**Client** отсутствует — server-to-server HTTP-клиент создаёт конкретная реализация.

## Ключевые абстракции

| Тип | Сборка | Роль |
|---|---|---|
| `CustomerBase : AggregateRoot<Guid>` | Domain | агрегат клиента; `InitializeCore`, `Rename`, `ChangeContacts`, `AssignOwner`, `Archive`, `Activate`, `Deactivate` |
| `ContactBase : AggregateRoot<Guid>` | Domain | агрегат контактного лица (`CustomerId`, `FullName`, `PositionId?` — ссылка на справочник должностей наследника, `Email?`, `Phone?`); `InitializeCore`, `Rename`, `ChangePosition`, `ChangeContacts`, `Remove` |
| `CustomerByEmail/ByIds/ByOwner/ActiveCustomers Specification<TCustomer>` | Domain | generic-спецификации фильтрации клиентов |
| `ContactsByCustomer/ByEmail Specification<TContact>` | Domain | generic-спецификации фильтрации контактов |
| `CustomerDtoBase`/`ContactDtoBase` + `Create`/`Update…RequestBase` | Contracts | абстрактные record (контакты — `string?`) |
| `CustomerDbContextBase<TContext, TCustomer, TContact>` | Infrastructure | `DbSet` клиентов и контактов в одной БД, применяет обе конфигурации через hook |
| `CustomerConfigurationBase<TCustomer>` / `ContactConfigurationBase<TContact>` | Infrastructure | таблица/схема, длины, индексы (Email, OwnerId, CustomerId), VO-конвертеры, `ConfigureCustom` hook |
| `ICustomerFactory`/`IContactFactory` | Application | `Create(...)` — заменяют `new` абстрактной сущности |
| `ICustomerProjector`/`IContactProjector` | Application | `ToDto(...)` — проекция вместо Mapster (VO → string без скрытой конфигурации) |
| generic CQRS клиента: `Create/Update/Archive` + `GetById/List` | Application | handler'ы закрываются конкретными типами наследника |
| generic CQRS контактов: `Add/Update/Remove` + `GetById/ListByCustomer` | Application | то же для контактных лиц |
| `CustomerEndpointsBase<…>` / `ContactEndpointsBase<…>` | Api | CRUD-маршруты; контакты вложены — `api/customers/{customerId}/contacts`; методы `virtual` |
| `CheetahCustomerApiModuleBase<…>` / `CheetahCustomerContactsApiModuleBase<…>` | Api | маппят эндпоинты в `OnApplicationInitialization` |

### Архитектурные приёмы абстрактности

- **Нельзя `new` абстрактную сущность** в generic-handler → фабрика `ICustomerFactory`.
- **`[Export]` source-gen работает только по закрытым типам** → открытые generic-handler'ы
  регистрируются вручную в extension-методах (`AddCustomerInfrastructure<>`, `AddCustomerApplication<>`).
- **Абстрактный DbContext нельзя мигрировать** → конкретный `DbContext`, design-time factory и
  миграции принадлежат наследнику; базовый `OnModelCreating` применяет конфигурацию через hook.
- **VO не протекают наружу** → в Contracts строки; конвертация string↔VO в фабрике/мутаторах
  сущности и через EF value converter в `CustomerConfigurationBase`.

## Extension-методы (точки регистрации у наследника)

```csharp
// Infrastructure: DbContext (клиенты + контакты), мигратор, PostgreSQL,
// IRepository<Customer, Guid> и IRepository<Contact, Guid>
services.AddCustomerInfrastructure<AppCustomerDbContext, Customer, Contact>();

// Application клиента: фабрика, проектор, закрытые CQRS-handler'ы
services.AddCustomerApplication<Customer, CreateCustomerRequest, UpdateCustomerRequest,
    CustomerDto, CustomerFactory, CustomerProjector>();

// Application контактов (опционально): фабрика, проектор, закрытые CQRS-handler'ы
services.AddCustomerContacts<Contact, CreateContactRequest, UpdateContactRequest,
    ContactDto, ContactFactory, ContactProjector>();
```

## Пример наследования («быстрая реализация»)

```csharp
// 1. Сущность
public sealed class Customer : CustomerBase
{
    public string? Inn { get; private set; }
    private Customer() { }
    public static Customer Create(CreateCustomerRequest r)
    {
        var c = new Customer();
        c.InitializeCore(Guid.NewGuid(), r.DisplayName, r.Email, r.Phone);
        c.Inn = r.Inn;
        return c;
    }
}

// 2. Contracts
public sealed record CustomerDto : CustomerDtoBase { public string? Inn { get; init; } }
public sealed record CreateCustomerRequest : CreateCustomerRequestBase { public string? Inn { get; init; } }
public sealed record UpdateCustomerRequest : UpdateCustomerRequestBase;

// 3. Фабрика + проектор
public sealed class CustomerFactory : ICustomerFactory<Customer, CreateCustomerRequest>
{ public Customer Create(CreateCustomerRequest r) => Customer.Create(r); }

public sealed class CustomerProjector : ICustomerProjector<Customer, CustomerDto>
{
    public CustomerDto ToDto(Customer c) => new()
    {
        Id = c.Id, DisplayName = c.DisplayName, Email = c.Email?.Value, Phone = c.Phone?.Value,
        Status = c.Status, CreatedAt = c.CreatedAt, Inn = c.Inn
    };
}

// 4. Контактное лицо: сущность, Contracts, фабрика, проектор, конфигурация
public sealed class Contact : ContactBase
{
    private Contact() { }
    public static Contact Create(Guid customerId, CreateContactRequest r)
    {
        var c = new Contact();
        c.InitializeCore(Guid.NewGuid(), customerId, r.FullName, r.PositionId, r.Email, r.Phone);
        return c;
    }
}
public sealed record ContactDto : ContactDtoBase;
public sealed record CreateContactRequest : CreateContactRequestBase;
public sealed record UpdateContactRequest : UpdateContactRequestBase;

public sealed class ContactFactory : IContactFactory<Contact, CreateContactRequest>
{ public Contact Create(Guid customerId, CreateContactRequest r) => Contact.Create(customerId, r); }

public sealed class ContactProjector : IContactProjector<Contact, ContactDto>
{
    public ContactDto ToDto(Contact c) => new()
    {
        Id = c.Id, CustomerId = c.CustomerId, FullName = c.FullName, PositionId = c.PositionId,
        Email = c.Email?.Value, Phone = c.Phone?.Value, CreatedAt = c.CreatedAt
    };
}
public sealed class ContactConfiguration : ContactConfigurationBase<Contact> { }

// 5. Конфигурация + DbContext клиента (миграции — у наследника)
public sealed class CustomerConfiguration : CustomerConfigurationBase<Customer>
{ protected override void ConfigureCustom(EntityTypeBuilder<Customer> b) => b.Property(x => x.Inn).HasMaxLength(12); }

public sealed class AppCustomerDbContext(DbContextOptions<AppCustomerDbContext> o)
    : CustomerDbContextBase<AppCustomerDbContext, Customer, Contact>(o)
{
    protected override IEntityTypeConfiguration<Customer> CreateCustomerConfiguration() => new CustomerConfiguration();
    protected override IEntityTypeConfiguration<Contact> CreateContactConfiguration() => new ContactConfiguration();
}

// 6. Api-модули наследника (клиенты + контакты)
public sealed class AppCustomerEndpoints
    : CustomerEndpointsBase<CreateCustomerRequest, UpdateCustomerRequest, CustomerDto> { }
public sealed class AppCustomerApiModule
    : CheetahCustomerApiModuleBase<AppCustomerEndpoints, CreateCustomerRequest, UpdateCustomerRequest, CustomerDto> { }

public sealed class AppContactEndpoints
    : ContactEndpointsBase<CreateContactRequest, UpdateContactRequest, ContactDto> { }
public sealed class AppContactsApiModule
    : CheetahCustomerContactsApiModuleBase<AppContactEndpoints, CreateContactRequest, UpdateContactRequest, ContactDto> { }

// 7. dotnet ef migrations add Initial  (+ IDesignTimeDbContextFactory) — в проекте наследника
```

## Ограничения

- **Миграции и `IDesignTimeDbContextFactory` — только у наследника.** В модуле их нет.
- Абстрактные Contracts нельзя забиндить/инстанцировать — нужны конкретные `sealed record`.
- Проекция `TCustomer → TDto` — через `ICustomerProjector` наследника (Mapster здесь не используется,
  чтобы не требовать скрытой регистрации VO-конвертеров).
- `Archive()` клиента ставит `Status = Archived` + `RemovedAt`; `Remove()` контакта ставит `RemovedAt`.
  Глобальный soft-delete query-filter по умолчанию **не** включён; `ListContactsByCustomer` сам
  отсекает удалённые (если не передан `includeRemoved: true`).
- `OwnerId` — логическая ссылка на пользователя Identity (без навигации/FK). Подписки на удаление
  пользователя из коробки нет — добавляет наследник при необходимости.
- Контакты живут в **той же БД**, что и клиенты (один `DbContext`). Отдельный модуль с собственной
  БД оправдан только если контакты нужны другим модулям независимо от клиента.
