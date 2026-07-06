# Этап 7. Резолвинг тенанта на входе (`Cheetah.AspNetCore.Tenants`)

> [← Бэклог](README.md) · Зависимости: [T-1.1](stage-1-current-tenant.md#t-11)–[T-1.3](stage-1-current-tenant.md#t-13),
> для T-7.2 — [T-5.1](stage-5-connection-resolver.md#t-51).

<a id="t-71"></a>
## T-7.1 Новая сборка: middleware + контрибьюторы — **L**

**Зависит от:** T-1.1, T-1.2, T-1.3.
**Проект:** новый `src/Cheetah.AspNetCore.Tenants` (+ тесты; по образцу других
`Cheetah.AspNetCore.*`).

**Состав:**
```csharp
public interface ITenantResolveContributor
{
    int Order { get; }
    // вернуть Id/Code кандидата или null; НЕ валидирует существование
    ValueTask<TenantResolveResult?> ResolveAsync(HttpContext ctx, CancellationToken ct);
}
public sealed record TenantResolveResult(Guid? Id, string? Code, string Source);
```

1. **ClaimTenantResolveContributor** (Order 10): `User.GetTenantId()`. Если пользователь
   аутентифицирован user-токеном и клейма нет — тенант null (хост-пользователь/админ).
   **Если клейм есть — остальные контрибьюторы не опрашиваются** (доверенный источник;
   `X-Tenant-Id` от пользователя тенанта A со значением B должен игнорироваться).
2. **HeaderTenantResolveContributor** (Order 20): `X-Tenant-Id` (Guid). Учитывается ТОЛЬКО
   для запросов с service-токеном (M2M, признак — тип клейма `client_id` без user-клеймов,
   согласовать с `Cheetah.Backend.ServiceAuth`) или для анонимных запросов, если явно
   включено опцией (`AllowHeaderForAnonymous`, default false — иначе публичный энумератор
   тенантов).
3. **SubdomainTenantResolveContributor** (Order 30): `{code}.{BaseDomain}` из опций
   (`TenantResolutionOptions.BaseDomain`); code → `ITenantStore.FindByCodeAsync`.
   Для анонимных страниц (логин, публичные Booking-страницы).
4. **TenantResolutionMiddleware**: первый не-null результат → `ITenantStore.FindAsync` →
   проверки: существует; `Status == Active` (Provisioning/Deactivated/Deleted → **404**,
   не 403 — не раскрывать существование тенанта; для Provisioning вернуть 503 c Retry-After —
   решить в PR, зафиксировать в README); затем `using currentTenant.Change(id, code)` на
   весь остаток пайплайна. Ничего не нашли → тенант не установлен (хост-скоуп) — это
   валидный сценарий (безтенантный режим и host-админка).
5. Порядок в пайплайне: **после** Authentication, **до** Authorization и эндпоинтов —
   зафиксировать в экстеншене `UseMultitenancy()` и README.
6. Логирование: пушить `TenantId` в logging scope
   (`logger.BeginScope`) — все логи запроса помечены тенантом.
7. Тесты (unit на контрибьюторы + integration через `WebApplicationFactory`):
   приоритет claim > header; header игнорируется при user-токене; subdomain для
   анонимного; неактивный тенант → 404; отсутствие тенанта → пайплайн работает в
   хост-скоупе.

**DoD:** сборка + ≥10 тестов + README (схема принятия решения — таблица источник/условие).

<a id="t-72"></a>
## T-7.2 Прогрев кэша строк в middleware — **S**

**Зависит от:** T-7.1, T-5.1.
**Файлы:** `TenantResolutionMiddleware`.

**Шаги:**
1. После успешного `Change`: если `cache.Get(tenantId, <любое-имя-модуля>) == null` —
   `await cache.PrewarmAsync(tenantId, ct)` (async-контекст middleware это позволяет;
   именно поэтому sync `Resolve` дальше по пайплайну всегда попадает в кэш).
   Дешёвый признак «прогрет ли» — отдельный маркер-ключ `tcs:{tenantId}:__warm`.
2. Конкурентный прогрев одного тенанта — схлопнуть через `Lazy`/`SemaphoreSlim` per-tenant
   (стандартная защита от cache stampede).
3. Тест: первый запрос тенанта прогревает, второй — нет (счётчик обращений к стору).

**DoD:** EF-путь никогда не промахивается мимо кэша после middleware.

<a id="t-73"></a>
## T-7.3 Identity: клейм тенанта при логине — **M**

**Зависит от:** T-1.3; координация с T-7.1.
**Файлы:** `Modules/Identity` (генерация JWT — там, где формируются клеймы),
`ApplicationClaimsPrincipalFactory` в `Cheetah.Core.Security`.

**Шаги:**
1. Решение из проектного дока: Identity — **тенантный** модуль. Значит на логине тенант
   уже отрезолвлен (subdomain/поле формы → middleware установил `ICurrentTenant`), юзер
   ищется в БД этого тенанта, а в токен пишутся `tenant_id` + `tenant_code` из
   `ICurrentTenant`.
2. Host-realm: пользователи платформенной админки живут в хостовой БД Identity
   (безтенантный дефолт) и получают токен БЕЗ tenant-клейма + permission `Tenancy.Manage`.
   Отдельной работы почти нет: хост-логин — это логин без тенант-контекста.
3. Refresh-токены: тенант зашит в сам токен — при refresh клейм переносится; проверить.
4. Тесты: токен тенантного юзера несёт клеймы; хост-юзер — нет; refresh сохраняет.

**DoD:** JWT — доверенный источник тенанта end-to-end (логин → клейм → middleware → контекст).
