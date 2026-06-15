# Cheetah.Modules.Leads.* — абстрактный шаблон-модуль «Лиды»

> **Тип:** base/template (каркас), а не готовый микросервис.
> Модуль поставляет **только абстрактные базовые типы и generic-хелперы**. Конкретное приложение
> наследует типы, генерирует миграции у себя и получает рабочий CRUD + lifecycle + конвертацию
> «из коробки», дописав ~6–7 классов. Сделан по образцу [`Cheetah.Modules.Customer`](../Customer/README.md)
> и [`Cheetah.Modules.Activities`](../Activities/README.md).
>
> Полный план и решения — [`docs/modules/leads.md`](../../../docs/modules/leads.md).

## Назначение

Лиды — входная воронка: захват «сырых» контактов, квалификация и **конвертация** в `Customer`
(+ опц. `Deal`). Жизненный цикл: `New → Working → Qualified → Converted | Disqualified`.

**Главное требование — расширяемость:** и сущность, и ViewModel наследуемы; приложение добавляет свои
поля без форка модуля. Дополнительно расширяемы **скоринг** и **конвертация**.

## Состав сборок и граф зависимостей

```
Leads.DomainEvents   → Core.Events                          (LeadCreated/Qualified/Disqualified/Converted)
Leads.Shared         → Core                                 (enum LeadStatus/LeadSource, константы)
Leads.Contracts      → Core + Contracts + Shared            (ABSTRACT DTO/Request + ConvertLeadResult)
Leads.Domain         → DomainEvents + Specification + SM     (abstract LeadBase, generic-спеки, ILeadConversionOrchestrator)
Leads.Infrastructure → Domain + EF + EF.PostgreSql          (abstract DbContextBase/ConfigBase с VO-конвертерами, AddLeadsInfrastructure<>)
Leads.Application     → Domain + Contracts + CQRS + Events   (generic handlers, ILeadFactory/Projector, AddLeadsApplication<>)
Leads.Api            → Application + Contracts + AspNetCore  (abstract LeadEndpointsBase<>, ApiModuleBase)
Tests: Domain.Tests (16), Application.Tests (11)
```

`Default` и `Client` отсутствуют — их создаёт наследник (нужны закрытые типы и реализация конвертации).

## Ключевые точки расширения

| Точка | Как |
|---|---|
| Сущность | `sealed class Lead : LeadBase` + поля; создание через `InitializeCore` в фабрике |
| ViewModel | `sealed record LeadDto : LeadDtoBase` + поля |
| Запросы | `Create/Update/ConvertLeadRequestBase` |
| Создание/проекция | `ILeadFactory`/`ILeadProjector` |
| Схема EF | `LeadConfigurationBase<TLead>` + `ConfigureCustom` hook (VO Email/Phone — конвертеры в базе) |
| **Скоринг** | `protected virtual int CalculateScore()` на `LeadBase` |
| **Конвертация** | порт `ILeadConversionOrchestrator` (Domain) — реализацию подключает наследник |
| Карман без миграций | колонка `Attributes (jsonb)` |

### Конвертация — порт `ILeadConversionOrchestrator`

Шаблон не знает, как создаются Customer/Deal, и не зависит от них. `ConvertLeadCommandHandler`:
проверяет, что лид `Qualified` → вызывает `ILeadConversionOrchestrator.ConvertAsync(...)` →
`lead.MarkConverted(customerId, dealId)` → событие `LeadConverted`. Реализацию порта даёт наследник:

- **прямой** orchestrator (`Customer.Client` + `Deals.Client`) с ручной компенсацией;
- **`Cheetah.Saga`** — событийная оркестрация с компенсациями (устойчивость к падениям);
- **event-only** — публикация события, создание делает Workflow/downstream.

## Extension-методы (точки регистрации у наследника)

```csharp
services.AddLeadsInfrastructure<AppLeadsDbContext, Lead>();

services.AddLeadsApplication<Lead, CreateLeadRequest, UpdateLeadRequest, ConvertLeadRequest,
    LeadDto, LeadFactory, LeadProjector>();

// реализацию конвертации регистрирует наследник:
services.AddScoped<ILeadConversionOrchestrator, DirectLeadConversionOrchestrator>();
```

Конечный автомат `LeadStatus` регистрируется самим `CheetahLeadsApplicationModule`.

## Пример наследования (фрагмент)

```csharp
public sealed class Lead : LeadBase
{
    public string? Industry { get; private set; }
    private Lead() { }
    public static Lead Create(CreateLeadRequest r)
    {
        var l = new Lead();
        l.InitializeCore(Guid.NewGuid(), r.FullName, r.Source, r.Email, r.Phone, r.Company, r.OwnerId);
        return l;
    }
    // при необходимости: protected override int CalculateScore() => ...;
}

public sealed record LeadDto : LeadDtoBase { public string? Industry { get; init; } }
public sealed record CreateLeadRequest : CreateLeadRequestBase { public string? Industry { get; init; } }
public sealed record UpdateLeadRequest : UpdateLeadRequestBase;
public sealed record ConvertLeadRequest : ConvertLeadRequestBase;

public sealed class LeadFactory : ILeadFactory<Lead, CreateLeadRequest>
{ public Lead Create(CreateLeadRequest r) => Lead.Create(r); }

public sealed class LeadProjector : ILeadProjector<Lead, LeadDto> { /* ToDto */ }

public sealed class LeadConfiguration : LeadConfigurationBase<Lead>
{ protected override void ConfigureCustom(EntityTypeBuilder<Lead> b) => b.Property(x => x.Industry).HasMaxLength(128); }

public sealed class AppLeadsDbContext(DbContextOptions<AppLeadsDbContext> o)
    : LeadsDbContextBase<AppLeadsDbContext, Lead>(o)
{ protected override IEntityTypeConfiguration<Lead> CreateLeadConfiguration() => new LeadConfiguration(); }

public sealed class AppLeadEndpoints
    : LeadEndpointsBase<CreateLeadRequest, UpdateLeadRequest, ConvertLeadRequest, LeadDto> { }
public sealed class AppLeadsApiModule
    : CheetahLeadsApiModuleBase<AppLeadEndpoints, CreateLeadRequest, UpdateLeadRequest, ConvertLeadRequest, LeadDto> { }

// + ILeadConversionOrchestrator, IDesignTimeDbContextFactory, dotnet ef migrations add Initial — у наследника
```

## Эндпоинты (по умолчанию, `api/leads`)

| Метод | Маршрут |
|---|---|
| POST | `/api/leads` |
| GET | `/api/leads?status=&source=&ownerId=` |
| GET | `/api/leads/{id}` |
| PUT | `/api/leads/{id}` |
| POST | `/api/leads/{id}/qualify` · `/disqualify` · `/convert` |

## События

`LeadCreated/Qualified/Disqualified/Converted` (`*IntegrationEvent`). Потребители: Activities,
Notification, аналитика; Saga-вариант конвертации потребляет `CustomerCreated`/`DealCreated`.

## Ограничения / follow-up

- Миграции, `IDesignTimeDbContextFactory` и реализация `ILeadConversionOrchestrator` — у наследника.
- `Client` и готовая сборка `.Default` — следующий инкремент.
- Публичный анонимный приём лидов (`api/public/leads` за `RateLimit` + captcha) — follow-up.
- Антидубль сейчас — отклонение по email; политика слияния — follow-up.
- События — через `IEventBus` после `SaveChangesAsync` (как в Customer/Activities); Outbox — follow-up.
- Коды ответов: not-found/дубль сейчас маппятся в 400 (`LeadValidationException`) — уточнить до 404/409.
