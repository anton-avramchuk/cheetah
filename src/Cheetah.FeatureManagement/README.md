# Cheetah.FeatureManagement

Лёгкая инфраструктурная абстракция фич-флагов: **движок вычисления** и **контракты**, без БД.
Подключается в **каждый** модуль/микросервис, которому нужно спрашивать «включена ли фича X?».

Источник определений (`IFeatureDefinitionProvider`) подключается **отдельно**, в зависимости от топологии:
- монолит / сервис FeatureManagement → `Cheetah.Modules.FeatureManagement.Infrastructure` (БД + кэш);
- микросервис-потребитель → `Cheetah.Modules.FeatureManagement.Client` (`UseRemoteReplica` — локальная реплика).

## Зависимости

- `Cheetah.Core` (модульность, DI-генератор);
- `Cheetah.Expressions.JsonLogic` (фильтр `JsonLogic` — переиспользует общий движок выражений);
- `Microsoft.AspNetCore.App` (endpoint-filter `RequireFeature`).

## Подключение

```csharp
[DependsOn(typeof(CrmFeatureManagementModule))]
public partial class MyConsumerModule : CrmModule { }
```

`CrmFeatureManagementModule` регистрирует `IFeatureManager` (Scoped) и встроенные `IFeatureFilter`
(Singleton). Без поставщика определений `IFeatureManager` будет возвращать значения по умолчанию (флаг
выключен) — поставщик добавляет Infrastructure или Client.

## Использование

```csharp
// 1. Императивно
if (await _features.IsEnabledAsync("deals.kanban-v2", ctx, ct)) { /* новый путь */ }

// 2. Вариант A/B
var variant = await _features.GetVariantAsync("pricing.experiment", ctx, ct);

// 3. Декларативно на эндпоинте (404, если фича выключена)
routes.MapGet("/api/deals/board", Handler).RequireFeature("deals.kanban-v2");
```

`FeatureContext` (кто/где спрашивает) строится из HTTP-запроса через `IHttpFeatureContextBuilder`
(по умолчанию — UserId из `NameIdentifier`, TenantId из claim `tenant_id`, роли из role-claims).

## Движок оценки

Стратегия **«первое сработавшее»** (`FeatureManager`):

1. флаг не зарегистрирован → **выкл**;
2. `Enabled == false` (kill-switch) → **выкл**, минуя весь таргетинг;
3. нет правил → **вкл** (раскатан на всех);
4. правила перебираются по `Order`; первое сработавшее определяет результат:
   - `Negate` (deny) → **выкл** (короткое замыкание),
   - иначе → **вкл** (+ `ResultVariant` для флага-варианта);
5. ни одно не сработало → **выкл**.

Percentage-rollout **стабилен** по `SHA-256(featureKey + subject) % 100 < percentage` — субъект не
«мигает» между запросами/инстансами. Варианты A/B распределяются детерминированно по весам.

## Точка расширения — `IFeatureFilter`

Встроенные фильтры: `Percentage`, `Users`, `Tenants`, `Roles`, `TimeWindow`, `JsonLogic`.
Свою стратегию таргетинга добавляют, **не трогая движок**:

```csharp
[Export(LifetimeType.Singleton, typeof(IFeatureFilter))]
public sealed class PlanTierFilter : IFeatureFilter
{
    public string Name => "PlanTier";   // = TargetingRule.FilterName
    public ValueTask<bool> EvaluateAsync(FeatureFilterContext ctx, CancellationToken ct) { /* ... */ }
}
```

## Порт `IFeatureDefinitionProvider`

Абстракция не знает, откуда берутся определения. Реализации:
- `CachedFeatureDefinitionProvider` (Infrastructure) — БД + `ICacheService`;
- `RemoteFeatureDefinitionProvider` (Client) — локальная in-memory реплика (pull + push по событиям).

Движок и потребительский код от выбора реализации **не зависят** — это и делает модуль пригодным как
для монолита, так и для микросервисов.

## Ключевые типы

| Тип | Назначение |
|---|---|
| `IFeatureManager` | `IsEnabledAsync` / `GetVariantAsync` |
| `FeatureContext` | контекст оценки (UserId/TenantId/Roles/Attributes) |
| `IFeatureFilter` | стратегия таргетинга (встроенная или пользовательская) |
| `IFeatureDefinitionProvider` / `FeatureDefinition` | порт к источнику определений + read-DTO движка |
| `FeatureValueType` | `Bool` / `Variant` |
| `RequireFeature(...)` | endpoint-filter |
