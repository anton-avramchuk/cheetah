# Этап 1. Ядро тенант-контекста (`Cheetah.Core.Tenants`)

> [← Бэклог](README.md) · Зависимости: нет (T-1.2 — после T-1.1).
> Открывает путь этапам 3 (события) и 5 (резолвер строк).

<a id="t-11"></a>
## T-1.1 `ICurrentTenant` + AsyncLocal-реализация — **M**

**Зависит от:** ничего.
**Файлы:** `src/Cheetah.Core.Tenants/` → `ICurrentTenant.cs`, `CurrentTenant.cs`,
`TenantScope.cs` (internal), тесты в `src/Cheetah.Core.Tests/` (или новый
`Cheetah.Core.Tenants.Tests` — по образцу соседних).

**Контракт:**
```csharp
public interface ICurrentTenant
{
    Guid? Id { get; }
    string? Code { get; }
    bool IsAvailable { get; }              // Id.HasValue
    IDisposable Change(Guid? id, string? code = null);
}
```

**Шаги:**
1. Реализация `CurrentTenant`: `private static readonly AsyncLocal<TenantScope?> _current` —
   по образцу `DbContextCreationContext` (`src/Cheetah.Core.EntityFramework/DbContextCreationContext.cs`).
   `Change` сохраняет предыдущий scope и возвращает `DisposeAction`, восстанавливающий его.
2. `[Export(LifetimeType.Singleton, typeof(ICurrentTenant))]` — состояние в AsyncLocal, не в инстансе,
   поэтому singleton корректен.
3. Регистрация — автоматически через Source Generator в `CrmTenantsCoreModule` (модуль уже есть,
   сделать `partial` + `RegisterServices`, как в других Core-модулях).
4. Юнит-тесты (обязательные сценарии):
   - вне `Change` → `Id == null`, `IsAvailable == false`;
   - `Change(A)` → внутри A; вложенный `Change(B)` → B; после Dispose → снова A; после
     внешнего Dispose → null;
   - `Change(null)` внутри `Change(A)` — «выход в хост-скоуп» (нужен для `[IgnoreMultiTenancy]`);
   - протекание через `await Task.Yield()` / `Task.Run` — значение сохраняется в async-потоке;
   - два параллельных `Task.Run` с разными `Change` не видят друг друга.

**DoD:** интерфейс + реализация + ≥6 тестов; README `Cheetah.Core.Tenants` дополнен разделом
«Амбиентный контекст».

<a id="t-12"></a>
## T-1.2 `ITenantStore`, `TenantInfo`, `MultitenancyOptions`, `NullTenantStore` — **S**

**Зависит от:** T-1.1.
**Файлы:** `src/Cheetah.Core.Tenants/` → `ITenantStore.cs`, `TenantInfo.cs`,
`MultitenancyOptions.cs`, `NullTenantStore.cs`.

**Контракты:**
```csharp
public sealed record TenantInfo(Guid Id, string Code, string Name, bool IsActive, TenantStatus Status);
public enum TenantStatus { Provisioning, Active, Deactivated, Deleted }

public interface ITenantStore
{
    ValueTask<TenantInfo?> FindAsync(Guid id, CancellationToken ct = default);
    ValueTask<TenantInfo?> FindByCodeAsync(string code, CancellationToken ct = default);
    ValueTask<IReadOnlyList<TenantInfo>> GetActiveAsync(CancellationToken ct = default);
}
```

**Шаги:**
1. `NullTenantStore` (всегда «тенантов нет») регистрируется дефолтом через `TryAdd` —
   модуль Tenancy позже перекроет своей реализацией. Так безтенантный хост работает без
   дополнительной конфигурации — это и есть «подключаемость».
2. `MultitenancyOptions` (`Multitenancy` секция): `DatabaseNamePrefix` (default `"t"`),
   `HostConnectionStringNames` (список имён строк, которые всегда хостовые — для Dapper/Mongo,
   где нет атрибута на типе), `ReconciliationInterval` (nullable, для этапа 6).
3. Перенести `TenantStatus` так, чтобы `TenantEntity` из скелета получил поле `Status`
   (сейчас у него только `IsActive`) — см. [T-4.2](stage-4-tenancy-module.md#t-42), здесь только enum.
4. Юнит-тестов минимум: `NullTenantStore` возвращает null/пусто.

**DoD:** контракты в Core, дефолтные регистрации, README обновлён.

<a id="t-13"></a>
## T-1.3 Клейм тенанта в `ApplicationClaimTypes` — **S**

**Зависит от:** ничего.
**Файлы:** `src/Cheetah.Core.Security/Claims/ApplicationClaimTypes.cs`,
`Extensions/ClaimsIdentityExtensions.cs`.

**Шаги:**
1. `public static string TenantId { get; set; } = "tenant_id";` и
   `public static string TenantCode { get; set; } = "tenant_code";`.
2. В `ClaimsIdentityExtensions` — `GetTenantId(this ClaimsIdentity/Principal): Guid?`
   (по образцу существующих экстеншенов).
3. Юнит-тест на парсинг клейма (валидный Guid / мусор / отсутствует).

**DoD:** клеймы и экстеншены готовы; выдачей клейма при логине займётся
[T-7.3](stage-7-aspnetcore.md#t-73).

<a id="t-14"></a>
## T-1.4 Атрибут `[IgnoreMultiTenancy]` — **S**

**Зависит от:** ничего.
**Файлы:** `src/Cheetah.Core.DataAccess/Attributes/IgnoreMultiTenancyAttribute.cs`.

**Шаги:**
1. `[AttributeUsage(AttributeTargets.Class)] public sealed class IgnoreMultiTenancyAttribute : Attribute`.
   Кладём в `Core.DataAccess` рядом с `ConnectionStringNameAttribute` — его видят и EF, и
   резолвер, без новых зависимостей.
2. Статический хелпер-кэш `IgnoreMultiTenancyAttribute.IsIgnored(Type)` c
   `ConcurrentDictionary<Type, bool>` (атрибут читается на горячем пути создания DbContext).

**DoD:** атрибут + хелпер; применение — в [T-5.3](stage-5-connection-resolver.md#t-53).
