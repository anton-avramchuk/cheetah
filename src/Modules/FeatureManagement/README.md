# Cheetah.Modules.FeatureManagement.* — фич-флаги (расширяемый шаблон)

Бизнес-модуль управления фич-флагами: хранилище, таргетинг, реестр, админка. Реализует порт
`IFeatureDefinitionProvider` из инфраструктурной абстракции [`Cheetah.FeatureManagement`](../../Cheetah.FeatureManagement/README.md)
(движок + контракты). Полный план — [`docs/modules/feature-management.md`](../../../docs/modules/feature-management.md).

Модуль сделан **расширяемым шаблоном** (как `Customer`/`Activities`): поставляет абстрактные базовые
типы + generic-хелперы; наследник дописывает `sealed`-типы со своими полями. Плюс сборка `.Default`
даёт рабочую реализацию «из коробки».

## Сборки

| Сборка | Роль |
|---|---|
| `DomainEvents` | `FeatureFlagCreated/Changed/Toggled` |
| `Shared` | `RolloutType`, `FeatureKeys`, `FeatureManagementConstants` |
| `Contracts` | абстрактные `FeatureFlagDtoBase`/`…RequestBase` + `FeatureDefinitionDescriptor` + DTO |
| `Domain` | `abstract FeatureFlagBase` + дети (`TargetingRule`/`FeatureVariantDef`/`TenantOverride`), спеки, `IFeatureFlagRepository` |
| `Infrastructure` | EF (`FeatureFlagConfigurationBase`/`DbContextBase`), `CachedFeatureDefinitionProvider` (БД+кэш), `FeatureCacheInvalidator` |
| `Application` | generic CQRS (создание, kill-switch, таргетинг, tenant-override, registry/sync, evaluate) |
| `Api` | `FeatureFlagEndpointsBase` + `ApiModuleBase` (реестр + админка + evaluate + definitions) |
| `Default` | sealed-типы + `FeatureManagementDbContext` + миграция + единый модуль «из коробки» |
| `Client` | `IFeatureCatalogClient` + `RemoteFeatureDefinitionProvider` (`UseRemoteReplica`) + регистрация флагов |

## Запуск «из коробки» (Default)

```csharp
[DependsOn(typeof(CheetahFeatureManagementDefaultModule))]
public partial class AppModule : CrmModule { }
```

Connection string — `FeatureManagement` (схема `features`). Подключает БД, репозитории, провайдер,
CQRS, эндпоинты `api/features/*` и подписывает инвалидатор кэша на шину.

## Расширение (свой набор вместо Default)

1. `sealed class FeatureFlag : FeatureFlagBase` (+ свои поля через расширенный `InitializeCore`);
2. `sealed record FeatureFlagDto : FeatureFlagDtoBase`, `CreateFeatureFlagRequest : CreateFeatureFlagRequestBase`;
3. `FeatureFlagFactory : IFeatureFlagFactory<…>`, `FeatureFlagProjector : IFeatureFlagProjector<…>`;
4. `FeatureManagementDbContext : FeatureManagementDbContextBase<…>` + конфиг `: FeatureFlagConfigurationBase<…>`
   (доп. поля/индексы — через `ConfigureCustom`) + миграция;
5. регистрация: `AddFeatureManagementInfrastructure<…>()` + `AddFeatureManagementApplication<…>()`;
6. эндпоинты: `: FeatureFlagEndpointsBase<…>` (методы `virtual`).

> Поведенческое расширение таргетинга — через plugin `IFeatureFilter` в абстракции (без правки модуля).

## Регистрация флагов потребителем (микросервис)

```csharp
services.AddFeatureManagementClient(o => o.BaseUrl = cfg["Features:Url"])
        .UseRemoteReplica()              // локальная in-memory реплика (горячий путь без сети)
        .RegisterFeatures(reg => reg
            .Add("deals", "Модуль сделок")
            .Add("deals.kanban-v2", "Kanban v2", x =>
            {
                x.ValueType = FeatureValueType.Bool;
                x.ParentKey = "deals";   // каскад: выключённый родитель гасит потомка
            }));
```

`FeatureClientHostedService` при старте регистрирует флаги в каталоге и делает первый pull реплики
(`ContinueOnFailure` — недоступность каталога не валит хост). Реплика обновляется по событиям
`FeatureFlagChanged/Toggled` с шины. Подробнее — раздел §2.1 плана.

Синхронизация идемпотентна и **не трогает** `Enabled`/таргетинг существующего флага — рестарт сервиса
не сбрасывает настройки админа. `ParentKey` — исключение: объявленный кодом родитель применяется и к
существующему флагу, чтобы иерархия воспроизводилась на любой БД. Дескриптор **без** родителя связь не
снимает: код её не объявлял, значит и не отменяет (родителя, выставленного руками в админке, рестарт
не ломает).

## Эндпоинты

- `POST /api/features/registry/sync`, `GET /api/features/registry` — реестр;
- `POST/GET /api/features`, `GET /api/features/{key}`, `POST /{key}/enable|disable`,
  `PUT /{key}/targeting`, `PUT /{key}/tenants/{tenantId}` — админка;
- `POST /api/features/evaluate` — батч-оценка; `GET /api/features/definitions` — снимок для реплики.

## Тесты

Абстракция — 16 (движок + фильтры); Domain — 6; Application — 5; Client — 4. Итого **31** зелёный.
