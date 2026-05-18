# Cheetah.Permissions

Permissions-инфраструктура для Cheetah CRM. **Не дублирует Identity** — встраивается в неё:
permissions хранятся как **claims** на `IdentityRole` (через `IdentityRoleClaim`) и/или
напрямую на `IdentityUser` (через `IdentityUserClaim`).

## Содержание

- [Архитектура](#архитектура)
- [Основные компоненты](#основные-компоненты)
- [Сценарий 1: монолит](#сценарий-1-монолит)
- [Сценарий 2: микросервисы](#сценарий-2-микросервисы)
- [Объявление permissions](#объявление-permissions)
- [Назначение permission'ов пользователю или роли](#назначение-permissionов-пользователю-или-роли)
- [Проверка permission'ов](#проверка-permissionов)
- [Каталог permissions](#каталог-permissions)
- [FAQ](#faq)

## Архитектура

```
┌─────────────────────────────────────────────────────────────────┐
│ Cheetah.Modules.Identity (source of truth: пользователи и роли) │
│                                                                  │
│   IdentityRole   IdentityRoleClaim  ─┐                           │
│   IdentityUser   IdentityUserClaim  ─┼─ claims ("permission",X) │
│                                      │                           │
└─────────────────────────────────────────────────────────────────┘
                          ▲
                          │ при логине Identity выписывает JWT
                          │ с claims из UserClaims + RoleClaims пользователя
                          ▼
┌─────────────────────────────────────────────────────────────────┐
│ Cheetah.Permissions (этот модуль) — проверка ClaimsPrincipal    │
│                                                                  │
│   [Permission]             → декларация в коде                  │
│   PermissionRegistry       → in-memory реестр объявлений         │
│   IPermissionAuthorizer    → ClaimsPrincipal.HasClaim(...)       │
│   ICurrentUserPermissions  → fast-path для DI в любом сервисе   │
│   PermissionPolicyProvider → app.MapPost(...).RequirePermission()│
└─────────────────────────────────────────────────────────────────┘
                          ▲                            ▲
                          │ local-sync                 │ remote-sync
                          ▼                            ▼
            ┌────────────────────────┐  ┌──────────────────────────┐
            │ Cheetah.Permissions    │  │ Cheetah.Permissions      │
            │   .Catalog (БД)        │  │   .Catalog.Client (HTTP) │
            │ Каталог объявленных    │  │ Микросервис при старте   │
            │ permissions для UI     │  │ POSTит свои permissions  │
            └────────────────────────┘  └──────────────────────────┘
```

## Основные компоненты

| Тип | Назначение |
|---|---|
| `[Permission("Key", "Description")]` | Декларативное объявление permission на классе/поле/свойстве |
| `PermissionRegistry` | In-memory реестр + `ScanAssemblies` (auto-discovery) |
| `PermissionDescriptor` / `PermissionDefinitionDto` | DTO одного permission'а |
| `PermissionConstants.PermissionClaimType` | `"permission"` — имя claim-типа в Identity |
| `IPermissionAuthorizer` | `Has` / `HasAll` / `HasAny` поверх `ClaimsPrincipal` (низкоуровневый) |
| `ICurrentUserPermissions` | Удобный wrapper: достаёт `ClaimsPrincipal` из `HttpContext`, методы `Has`/`HasAll`/`HasAny`/`Require` без principal'a |
| `PermissionRequirement` + `PermissionAuthorizationHandler` | ASP.NET Core authorization |
| `PermissionPolicyProvider` | Динамические policies вида `"permission:Documents.Sign"` |
| `EndpointExtensions.RequirePermission()` | `app.MapPost(...).RequirePermission("Documents.Sign")` |

---

## Сценарий 1: монолит

В монолите всё работает в одном процессе. Подключаете три модуля в bootstrap-проекте:
- `CrmPermissionsModule` — ядро (везде).
- `CrmPermissionsCatalogModule` — каталог permissions в БД (опционально, для admin-UI).
- `CrmPermissionsCatalogApiModule` — REST API каталога (опционально).

### Шаг 1: подключение

```csharp
// Bootstrap module
[DependsOn(typeof(CrmPermissionsModule))]
[DependsOn(typeof(CrmPermissionsCatalogModule))]      // если нужен каталог в БД
[DependsOn(typeof(CrmPermissionsCatalogApiModule))]   // если нужен REST API каталога
public partial class MyMonolithModule : CrmModule { }
```

`appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Permissions": "Host=localhost;Database=permissions;Username=postgres;Password=..."
  }
}
```

### Шаг 2: объявление permissions в своих модулях

```csharp
public static class ContractPermissions
{
    [Permission(Sign,       "Подписать договор")]
    public const string Sign = "Documents.Contract.Sign";

    [Permission(Cancel,     "Отменить договор")]
    public const string Cancel = "Documents.Contract.Cancel";
}
```

Атрибут `[Permission]` можно вешать на класс (один permission на класс) или на поле/свойство
(один permission per поле). Атрибут `Module` опционален — по умолчанию имя сборки.

### Шаг 3: автоматическая регистрация в каталоге

При старте `LocalRegistrySyncService` (hosted из `CrmPermissionsCatalogModule`):
1. Сканит все загруженные сборки через `PermissionRegistry.ScanAssemblies`.
2. Группирует по `Module`.
3. Сохраняет в таблицу `permissions.Permissions` через `IRepository<PermissionDefinition, string>`.

Никакого ручного действия не требуется. Идемпотентно при последующих запусках.

### Шаг 4: назначение permission через Identity

См. [Назначение permission'ов](#назначение-permissionов-пользователю-или-роли).

### Шаг 5: проверка permission

См. [Проверка permission'ов](#проверка-permissionов).

---

## Сценарий 2: микросервисы

В микросервисной среде каждый сервис содержит свой набор permissions, но
**каталог — централизованный**, чтобы admin-UI видел все доступные permissions
из всех сервисов.

Структура:
- **`Permissions-admin`** (один сервис, обычно рядом с Identity) — подключает Catalog + Catalog.Api.
- **Каждый другой микросервис** — подключает `CrmPermissionsModule` + `CrmPermissionsCatalogClientModule`.

### Сервис Permissions-admin

```csharp
[DependsOn(typeof(CrmPermissionsModule))]
[DependsOn(typeof(CrmPermissionsCatalogModule))]
[DependsOn(typeof(CrmPermissionsCatalogApiModule))]
public partial class PermissionsAdminBootstrapModule : CrmModule { }
```

Поднимает REST API:
- `POST /api/permissions/catalog/sync` — принимает permissions от микросервисов.
- `GET /api/permissions/catalog?module=X` — список permissions (для UI).

### Любой другой микросервис

```csharp
[DependsOn(typeof(CrmPermissionsModule))]
[DependsOn(typeof(CrmPermissionsCatalogClientModule))]
public partial class MyServiceModule : CrmModule { }
```

`appsettings.json`:
```json
{
  "Permissions": {
    "CatalogClient": {
      "BaseUrl": "https://permissions-admin.internal/",
      "Timeout": "00:00:05",
      "ContinueOnFailure": true
    }
  }
}
```

При старте `RemoteRegistrySyncService`:
1. Сканит свои сборки.
2. POSTит permissions модуля в `/api/permissions/catalog/sync` админ-сервиса.
3. Если `ContinueOnFailure=true`, при недоступности catalog'а — warning в логи; сервис продолжает стартовать.
4. Если `ContinueOnFailure=false`, исключение упадёт в `IHostedService.StartAsync` и хост не поднимется.

### Машинная авторизация sync-эндпоинта

`POST /api/permissions/catalog/sync` защищён `RequirePermission(CatalogPermissions.Sync)`.
Микросервис должен ходить туда с JWT, имеющим claim `("permission", "Permissions.Catalog.Sync")`.
Реализация — на ваше усмотрение: machine-to-machine OAuth, выделенный сервис-аккаунт в Identity и т.п.

---

## Объявление permissions

Permission — это **строковый ключ** (рекомендация: `<Module>.<Entity>.<Action>`).

### Объявление через атрибут

```csharp
public static class ContractPermissions
{
    // Атрибут на поле — самый частый случай.
    [Permission(Sign,   "Подписать договор")]
    public const string Sign = "Documents.Contract.Sign";

    [Permission(Cancel, "Отменить договор")]
    public const string Cancel = "Documents.Contract.Cancel";
}

// Атрибут на классе — когда permission один.
[Permission("Reports.Daily.View", "Просмотр ежедневных отчётов")]
public static class DailyReports { }
```

### Уточнение модуля

По умолчанию `Module` берётся из имени сборки (`asm.GetName().Name`). Чтобы переопределить:

```csharp
[Permission("Documents.Sign", "Подписать", Module = "Documents")]
public const string Sign = "Documents.Sign";
```

### Ручная регистрация

Если нужно зарегистрировать что-то без атрибута (например, динамические permissions):

```csharp
public class MyStartupTask
{
    public MyStartupTask(PermissionRegistry registry)
    {
        registry.Add("Reports.Custom.View", "Просмотр кастомных отчётов", "Reports");
    }
}
```

---

## Назначение permission'ов пользователю или роли

Permissions **хранятся как claims в Identity**. Любой инструмент Identity для работы с claims
автоматически работает с permissions.

### Назначить permission роли

```csharp
public class GrantPermissionToRoleHandler(RoleManager<IdentityRole> roleManager)
{
    public async Task Grant(Guid roleId, string permissionKey)
    {
        var role = await roleManager.FindByIdAsync(roleId.ToString());
        await roleManager.AddClaimAsync(role!,
            new Claim(PermissionConstants.PermissionClaimType, permissionKey));
    }
}
```

Все пользователи этой роли при следующем логине получат claim в JWT.

### Назначить permission напрямую пользователю

```csharp
public class GrantPermissionToUserHandler(UserManager<IdentityUser> userManager)
{
    public async Task Grant(Guid userId, string permissionKey)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        await userManager.AddClaimAsync(user!,
            new Claim(PermissionConstants.PermissionClaimType, permissionKey));
    }
}
```

Прямые claims пользователя приоритетнее claims роли — стандартное поведение ASP.NET Core Identity.

### Отозвать

Симметрично — `RemoveClaimAsync` с тем же `Claim`.

### Важно: JWT и propagation claims

Чтобы permissions попали в `ClaimsPrincipal` после логина, Identity при выписке JWT должен
включать **и user claims, и role claims** в токен. Это поведение по умолчанию для
`SignInManager` / `UserManager.GetClaimsAsync` + role claims. Если вы кастомно собираете JWT —
не забудьте добавить и те, и другие.

После изменения permissions у роли существующие JWT не обновляются автоматически — изменения
вступают в силу при следующем логине (или при refresh JWT).

---

## Проверка permission'ов

Три способа, от удобного к низкоуровневому.

### 1. Через endpoint policy — `.RequirePermission()`

Самое простое и декларативное. Endpoint просто отказывает в 403, если permission'а нет:

```csharp
app.MapPost("/api/contracts/{id}/sign", SignContract)
   .RequirePermission(ContractPermissions.Sign);

app.MapDelete("/api/contracts/{id}", DeleteContract)
   .RequirePermission(ContractPermissions.Cancel);
```

Под капотом — `PermissionPolicyProvider` создаёт policy с `PermissionRequirement`,
`PermissionAuthorizationHandler` проверяет `user.HasClaim("permission", value)`.

### 2. В Command/Query handler — `ICurrentUserPermissions`

В Application-слое (или в любом сервисе) внедряется `ICurrentUserPermissions`.
**Не требует знания про HttpContext** — обёртка достаёт текущего пользователя сама.

```csharp
public class SignContractCommandHandler : ICommandHandler<SignContractCommand>
{
    private readonly ICurrentUserPermissions _perms;
    private readonly IRepository<Contract, Guid> _repository;

    public SignContractCommandHandler(
        ICurrentUserPermissions perms,
        IRepository<Contract, Guid> repository)
    {
        _perms = perms;
        _repository = repository;
    }

    public async ValueTask HandleAsync(SignContractCommand cmd, CancellationToken ct)
    {
        // 1) Простая проверка
        if (!_perms.Has(ContractPermissions.Sign))
            throw new ForbiddenException();

        // 2) Или удобный guard (бросает UnauthorizedAccessException)
        _perms.Require(ContractPermissions.Sign);

        // 3) Несколько прав
        if (!_perms.HasAny(new[] { ContractPermissions.Sign, "Admin.Override" }))
            throw new ForbiddenException();

        // ... бизнес-логика
    }
}
```

**Когда у HTTP-контекста нет пользователя** (background-сервис, фоновое задание) —
все методы возвращают `false`. Для таких мест используйте `IPermissionAuthorizer`
с явным principal'ом.

### 3. Низкоуровнево — `IPermissionAuthorizer`

Если нужно проверить permission для конкретного `ClaimsPrincipal` (например, переданного
извне, или из cached JWT):

```csharp
public class TokenInspector(IPermissionAuthorizer authorizer)
{
    public bool CanSign(ClaimsPrincipal principal)
        => authorizer.Has(principal, ContractPermissions.Sign);
}
```

---

## Каталог permissions

Каталог — **опциональный** компонент. Permissions проверяются через claims вне зависимости от него.
Каталог нужен для UI: «админ выбирает permission из списка, чтобы назначить роли».

Подробности — в [`Cheetah.Permissions.Catalog/README.md`](../Cheetah.Permissions.Catalog/README.md)
и [`Cheetah.Permissions.Catalog.Client/README.md`](../Cheetah.Permissions.Catalog.Client/README.md).

---

## FAQ

**Q: Что если permission'а нет в JWT, но он есть у пользователя в БД?**
A: JWT — снимок claims на момент выписки. Изменения вступают в силу при следующем логине.
Если нужна реал-тайм проверка — настройте короткий TTL JWT + refresh token.

**Q: Можно ли иерархические permissions (`Documents.*`)?**
A: Из коробки нет. `Has("Documents.*")` не сработает. При необходимости — реализуйте
собственный `IPermissionAuthorizer` поверх `IPermissionAuthorizer` (`Has` идёт через wildcard-match).

**Q: Как ограничить permission scope'ом (например, «только для своего отдела»)?**
A: Это уже не RBAC, а ABAC. Делается комбинацией: permission в JWT + бизнес-логика в handler'е
проверяет `currentUser.DepartmentId == document.DepartmentId`. `ICurrentUserPermissions` сюда
не лезет — это «можно ли в принципе», а не «можно ли с конкретной сущностью».

**Q: Composite policy provider — Permissions перетирает существующего `IAuthorizationPolicyProvider`?**
A: Да, через `services.Replace`. Подключайте `CrmPermissionsModule` **после** Identity / любого
модуля, который мог зарегистрировать кастомный provider. Если у вас несколько кастомных
provider'ов одновременно — нужно реализовать собственный composite, который умеет fallback'ом
между всеми. На текущем этапе проекта этого нет.

**Q: Microsoft.AspNetCore.Authorization не в пакетах — откуда `[Authorize]` и `AddAuthorization`?**
A: Через `<FrameworkReference Include="Microsoft.AspNetCore.App" />` — все ASP.NET Core API
доступны из shared framework, отдельный nuget не нужен.
