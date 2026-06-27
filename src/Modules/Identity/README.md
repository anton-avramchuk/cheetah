# Cheetah.Modules.Identity.* — базовый модуль аутентификации/пользователей

> **Тип:** base/template (каркас), а не готовый микросервис.
> Модуль поставляет **абстрактные расширяемые контракты + доменные сущности + обобщённые
> CQRS-хендлеры и эндпоинты**. Реальное приложение наследует типы своими классами (можно
> добавить поля) и подключает всё одним вызовом `AddCrmIdentity<…>().WithUsers<…>().WithRoles<…>()`.

## Идея расширяемости

Identity — базовый модуль. Почти весь вертикальный срез абстрактный/дженерик, хост подставляет
свои конкретные типы. Расширение идёт по двум осям:

```
запись:  TCreateRequest → TCreateCommand → createUser(cmd) → TUser (доменная сущность)
         TUpdateRequest → TUpdateCommand → applyUserChanges(user, cmd)
чтение:  TUser → TUserModel/TUserDetailModel → TUserGridVm/TUserDetailVm
```

Абстрактны: `CreateUserRequest`/`UpdateUserRequest`/`CreateRoleRequest`/`UpdateRoleRequest`,
`UserGridViewModel`/`UserDetailViewModel`/`RoleGridViewModel`/`RoleViewModel`, команды
Create/Update, доменные `IdentityUser<TRole>`/`IdentityRole`, `CheetahIdentityDbContext<…>`.
Конкретны (хост не расширяет): запросы GetById/Grid/Delete и команды Delete.

## Сборки

```
Identity.DomainEvents   → Core.Events                         (UserCreated/NameChanged/Deleted)
Identity.Contracts      → Core + Contracts                    (ABSTRACT Request/ViewModel + конкретные Get/Delete/Login/Token)
Identity.Domain         → DomainEvents                        (abstract IdentityUser<TRole>/IdentityRole + claims)
Identity.Infrastructure → Domain + EF + EF.PostgreSql         (abstract CheetahIdentityDbContext<…>, UserManager/RoleManager/Store, AddIdentityContext<…>)
Identity.Application    → Domain + Contracts + CQRS + Events   (abstract команды; sealed generic хендлеры + стратегии-делегаты)
Identity.Api            → Application + Contracts + Backend.* + AspNetCore   (AddCrmIdentity builder, дженерик-регистратор эндпоинтов, JWT/JWKS, service-token)
Identity.Mapster        → Application + Contracts + Mapping.Mapster          (профиль только для auth; остальное — по конвенции)
Identity.Mapping        → Application + Contracts                            (генерируемые мапперы только для auth)
Identity.Client         → Contracts + Backend.ServiceAuth                    (S2S IIdentityUsersClient + IdentityUserSummary)
Identity.Blazor         → AspNetCore.Blazor.*                                (UI-срез)
Tests: Application.Tests (57), Client.Tests (2)
```

## Подключение в хосте: `AddCrmIdentity`

`AddCrmIdentity` (сборка `Identity.Api`, namespace `Cheetah.Modules.Identity.Api.Registration`) —
единая точка входа. Регистрирует Identity-инфраструктуру (`AddIdentityContext`), обобщённые
CQRS-хендлеры под типы хоста, стратегии сборки сущностей и **замыкания регистрации эндпоинтов**.
Сами маршруты регистрируются модулем `CheetahIdentityApiModule` в `OnApplicationInitialization`.

```csharp
using Cheetah.Modules.Identity.Api.Registration;

services.AddCrmIdentity<AppUser, AppRole, AppIdentityDbContext>(o =>
        {
            o.Password.RequiredLength = 8;
            // ... любые IdentityOptions
        })
    .WithUsers<
        AppCreateUserRequest, AppCreateUserCommand,
        AppUpdateUserRequest, AppUpdateUserCommand,
        AppUserModel, AppUserDetailModel,
        AppUserGridViewModel, AppUserDetailViewModel>(
        createUser:        cmd => AppUser.Create(cmd.UserName, cmd.Email, cmd.Department),
        applyUserChanges: (user, cmd) => user.SetDepartment(cmd.Department)) // опционально
    .WithRoles<
        AppCreateRoleRequest, AppCreateRoleCommand,
        AppUpdateRoleRequest, AppUpdateRoleCommand,
        AppRoleModel,
        AppRoleGridViewModel, AppRoleViewModel>(
        createRole: cmd => AppRole.Create(cmd.Name));
```

### Сигнатуры

```csharp
CrmIdentityBuilder<TUser,TRole,TDbContext> AddCrmIdentity<TUser,TRole,TDbContext>(
    this IServiceCollection services, Action<IdentityOptions>? configureIdentity = null)
    where TDbContext : CheetahIdentityDbContext<TDbContext,TUser,TRole>
    where TRole : IdentityRole
    where TUser : IdentityUser<TRole>;

// .WithUsers<TCreateRequest,TCreateCommand,TUpdateRequest,TUpdateCommand,
//            TUserModel,TUserDetailModel,TUserGridVm,TUserDetailVm>(
//     Func<TCreateCommand,TUser> createUser,
//     Action<TUser,TUpdateCommand>? applyUserChanges = null)

// .WithRoles<TCreateRequest,TCreateCommand,TUpdateRequest,TUpdateCommand,
//            TRoleModel,TRoleGridVm,TRoleVm>(
//     Func<TCreateCommand,TRole> createRole,
//     Action<TRole,TUpdateCommand>? applyRoleChanges = null)
```

> `applyUserChanges`/`applyRoleChanges` — для **дополнительных** полей. Базовые
> (UserName/Email/security stamp, Name) применяются всегда стратегией по умолчанию.

## Что пишет хост

1. Доменные `AppUser : IdentityUser<AppRole>`, `AppRole : IdentityRole` (публичные фабрики/методы).
2. `AppIdentityDbContext : CheetahIdentityDbContext<AppIdentityDbContext, AppUser, AppRole>` + EF-конфигурации сущностей.
3. Конкретные record-ы: `App*Request` (наследуют абстрактные Create/Update), `App*Command`, `App*Model`, `App*ViewModel` (наследуют абстрактные VM) — с нужными доп. полями.
4. Провайдер БД (`UseNpgsql` + строка подключения) и миграции — как у всех модулей, builder в это не лезет.
5. Один вызов `AddCrmIdentity<…>().WithUsers<…>().WithRoles<…>()` + `[DependsOn(typeof(CheetahIdentityApiModule))]` у модуля хоста.

Хендлеры, маппинги (по конвенции Mapster), эндпоинты и логин/JWT — уже в Identity, дописывать не нужно.

## HTTP-эндпоинты

| Метод | Маршрут | Назначение |
|---|---|---|
| POST | `/api/users` `/api/roles` | создать (201 + Location на GetById) |
| PUT | `/api/users/{id}` `/api/roles/{id}` | обновить (204) |
| DELETE | `/api/users/{id}` `/api/roles/{id}` | удалить (204) |
| GET | `/api/users/{id}` `/api/roles/{id}` | по id (200/404) |
| GET | `/api/users` `/api/roles` | грид (пагинация/сортировка/фильтр) |
| POST | `/api/auth/login` | логин → JWT |
| POST | `/api/auth/service-token` | сервисный токен (client_credentials) |
| GET | `/.well-known/jwks.json`, `/.well-known/openid-configuration` | при RS256 |
| GET | `/api/auth/public-key` | при подключённом `CrmBackendRsaModule` |

## Server-to-server клиент

`Identity.Client` даёт `IIdentityUsersClient.GetUsersAsync()` → `IReadOnlyList<IdentityUserSummary>`
(конкретный transport-DTO `Id/UserName/Email`; расширенные поля grid-ViewModel хоста при
десериализации отбрасываются — потребителям не нужно знать тип хоста). Подключается модулем
`CheetahIdentityClientModule` (нужна секция `Identity:Client` и настроенный `ServiceAuth`).
Используется, например, фоновыми синками Teams/Tags.

## Тесты

```bash
dotnet test src/Modules/Identity/Cheetah.Modules.Identity.Application.Tests
dotnet test src/Modules/Identity/Cheetah.Modules.Identity.Client.Tests
```
