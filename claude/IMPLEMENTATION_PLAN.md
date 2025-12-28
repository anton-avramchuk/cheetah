# Cheetah CRM - План реализации базовых модулей

## 🎯 Цель

Реализовать 4 базовых модуля для CRM системы:
1. **Tenants** - управление тенантами, мультитенантность
2. **Features** - feature flags для тенантов
3. **Permissions** - RBAC система прав
4. **Identity** - пользователи и аутентификация

## 📊 Порядок реализации

```
Phase 1: Tenants (база для всего)
    ↓
Phase 2: Identity (пользователи)
    ↓
Phase 3: Permissions (права доступа)
    ↓
Phase 4: Features (feature flags)
```

---

# Phase 1: Cheetah.Tenants Module

**Цель:** Создать систему мультитенантности с tenant-per-database подходом.

## 1.1 Подготовка структуры проекта

### Задача 1.1.1: Создать структуру папок
```
src/Cheetah.Tenants/
├── Cheetah.Tenants.Domain/
├── Cheetah.Tenants.Application/
├── Cheetah.Tenants.DataAccess/
├── Cheetah.Tenants.Api/
├── Cheetah.Tenants.Shared/
└── Cheetah.Tenants.Frontend/
```

**Действия:**
- [ ] Создать папку `src/Cheetah.Tenants/`
- [ ] Создать 6 подпапок для каждого слоя
- [ ] Создать `.csproj` файлы для каждого проекта
- [ ] Добавить проекты в `Cheetah.slnx`

**Время:** 30 минут

---

## 1.2 Domain Layer

### Задача 1.2.1: Создать Domain модели

**Файлы:**
- `Entities/Tenant.cs`
- `Entities/TenantConnectionString.cs`
- `ValueObjects/Subdomain.cs`

**Tenant.cs:**
```csharp
public class Tenant : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; }
    public string NormalizedName { get; private set; }
    public string? Subdomain { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? ActivatedAt { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<TenantConnectionString> ConnectionStrings { get; private set; }

    public static Tenant Create(string name, string? subdomain = null);
    public void Activate();
    public void Deactivate();
    public void AddConnectionString(string name, string connectionString);
}
```

**Действия:**
- [ ] Создать `Tenant` aggregate root
- [ ] Создать `TenantConnectionString` entity
- [ ] Добавить валидацию в методы
- [ ] Добавить private конструктор для EF Core

**Время:** 1 час

### Задача 1.2.2: Создать Domain Events

**Файлы:**
- `Events/TenantCreatedEvent.cs`
- `Events/TenantActivatedEvent.cs`
- `Events/TenantDeactivatedEvent.cs`
- `Events/TenantDatabaseCreatedEvent.cs`

**Действия:**
- [ ] Создать все события как `record`
- [ ] Наследовать от `EventBase`
- [ ] Добавить необходимые свойства

**Время:** 30 минут

### Задача 1.2.3: Создать Module класс

**Файл:** `CrmTenantsDomainModule.cs`

**Действия:**
- [ ] Создать partial class наследующийся от `CrmModule`
- [ ] Добавить `[DependsOn(typeof(CrmDomainModule))]`

**Время:** 15 минут

---

## 1.3 Application Layer

### Задача 1.3.1: Создать Commands

**Файлы:**
- `Commands/CreateTenantCommand.cs`
- `Commands/CreateTenantCommandHandler.cs`
- `Commands/ActivateTenantCommand.cs`
- `Commands/ActivateTenantCommandHandler.cs`
- `Commands/CreateTenantDatabaseCommand.cs`
- `Commands/CreateTenantDatabaseCommandHandler.cs`

**Действия:**
- [ ] Создать все команды как `record`
- [ ] Реализовать handlers с `[Export]` атрибутом
- [ ] Добавить публикацию событий после выполнения
- [ ] Добавить обработку ошибок

**Время:** 2 часа

### Задача 1.3.2: Создать Queries

**Файлы:**
- `Queries/GetTenantByIdQuery.cs`
- `Queries/GetTenantByIdQueryHandler.cs`
- `Queries/GetTenantBySubdomainQuery.cs`
- `Queries/GetTenantBySubdomainQueryHandler.cs`
- `Queries/GetAllTenantsQuery.cs`
- `Queries/GetAllTenantsQueryHandler.cs`

**Действия:**
- [ ] Создать все запросы как `record`
- [ ] Реализовать handlers с маппингом в ViewModels
- [ ] Использовать EF Core Select для проекции

**Время:** 1.5 часа

### Задача 1.3.3: Создать Services

**Файлы:**
- `Services/ITenantResolver.cs`
- `Services/TenantResolver.cs`
- `Services/ICurrentTenant.cs`
- `Services/CurrentTenant.cs`
- `Services/ITenantStore.cs`
- `Services/TenantStore.cs`
- `Services/ITenantDatabaseManager.cs`
- `Services/TenantDatabaseManager.cs`

**ITenantResolver:**
```csharp
public interface ITenantResolver
{
    Task<Guid?> ResolveTenantIdAsync(HttpContext context, CancellationToken ct = default);
}
```

**ICurrentTenant:**
```csharp
public interface ICurrentTenant
{
    Guid? Id { get; }
    string? Name { get; }
    bool IsAvailable { get; }
    IDisposable Change(Guid? tenantId);
}
```

**ITenantDatabaseManager:**
```csharp
public interface ITenantDatabaseManager
{
    Task CreateDatabaseAsync(Guid tenantId, CancellationToken ct = default);
    Task MigrateDatabaseAsync(Guid tenantId, CancellationToken ct = default);
    Task DeleteDatabaseAsync(Guid tenantId, CancellationToken ct = default);
}
```

**Действия:**
- [ ] Реализовать TenantResolver (subdomain + header)
- [ ] Реализовать CurrentTenant с AsyncLocal
- [ ] Реализовать TenantStore для доступа к данным
- [ ] Реализовать TenantDatabaseManager для управления БД
- [ ] Добавить `[Export]` ко всем реализациям

**Время:** 3 часа

### Задача 1.3.4: Создать Middleware

**Файл:** `Middleware/TenantResolverMiddleware.cs`

**Действия:**
- [ ] Создать middleware для резолвинга тенанта
- [ ] Интегрировать с ITenantResolver
- [ ] Установить ICurrentTenant

**Время:** 1 час

### Задача 1.3.5: Создать Module класс

**Файл:** `CrmTenantsApplicationModule.cs`

**Действия:**
- [ ] Создать module с зависимостями
- [ ] `[DependsOn(typeof(CrmTenantsDomainModule))]`
- [ ] `[DependsOn(typeof(CrmBackendCQRSModule))]`

**Время:** 15 минут

---

## 1.4 DataAccess Layer

### Задача 1.4.1: Создать DbContext

**Файл:** `TenantsDbContext.cs`

**Действия:**
- [ ] Создать DbContext наследующийся от `CrmDbContext`
- [ ] Добавить DbSet для Tenant и TenantConnectionString
- [ ] Применить конфигурации из assembly

**Время:** 30 минут

### Задача 1.4.2: Создать Entity Configurations

**Файлы:**
- `Configurations/TenantConfiguration.cs`
- `Configurations/TenantConnectionStringConfiguration.cs`

**Действия:**
- [ ] Настроить таблицы, ключи, индексы
- [ ] Добавить уникальный индекс на Subdomain
- [ ] Добавить `builder.Ignore(t => t.DomainEvents)`
- [ ] Настроить связи между сущностями

**Время:** 1 час

### Задача 1.4.3: Создать Migrations

**Действия:**
- [ ] Создать initial migration
- [ ] Проверить сгенерированный код
- [ ] Применить migration к тестовой БД

**Время:** 30 минут

### Задача 1.4.4: Создать Module класс

**Файл:** `CrmTenantsDataAccessModule.cs`

**Действия:**
- [ ] Создать module с регистрацией DbContext
- [ ] Настроить ConnectionString из конфигурации
- [ ] `[DependsOn(typeof(CrmTenantsDomainModule))]`
- [ ] `[DependsOn(typeof(CrmEntityFrameworkMsSqlModule))]`

**Время:** 30 минут

---

## 1.5 Shared Layer

### Задача 1.5.1: Создать ViewModels

**Файлы:**
- `ViewModels/TenantViewModel.cs`
- `ViewModels/TenantDetailsViewModel.cs`
- `ViewModels/TenantConnectionStringViewModel.cs`

**Действия:**
- [ ] Создать все ViewModels как классы без зависимостей
- [ ] Добавить валидационные атрибуты если нужно

**Время:** 30 минут

### Задача 1.5.2: Создать Request DTOs

**Файлы:**
- `Requests/CreateTenantRequest.cs`
- `Requests/UpdateTenantRequest.cs`
- `Requests/ActivateTenantRequest.cs`

**Действия:**
- [ ] Создать Request классы
- [ ] Добавить валидационные атрибуты

**Время:** 30 минут

### Задача 1.5.3: Создать Constants

**Файл:** `Constants/TenantConstants.cs`

**Действия:**
- [ ] Добавить константы (min/max длины, регулярки для subdomain)

**Время:** 15 минут

---

## 1.6 Api Layer

### Задача 1.6.1: Создать Controller

**Файл:** `Controllers/TenantsController.cs`

**Endpoints:**
- `POST /api/tenants` - создать тенант
- `GET /api/tenants/{id}` - получить тенант
- `GET /api/tenants` - получить все тенанты
- `PUT /api/tenants/{id}` - обновить тенант
- `POST /api/tenants/{id}/activate` - активировать
- `POST /api/tenants/{id}/deactivate` - деактивировать
- `DELETE /api/tenants/{id}` - удалить (soft delete)

**Действия:**
- [ ] Создать TenantsController
- [ ] Реализовать все endpoints через Dispatcher
- [ ] Добавить Swagger атрибуты
- [ ] Добавить валидацию

**Время:** 2 часа

### Задача 1.6.2: Создать Module класс

**Файл:** `CrmTenantsApiModule.cs`

**Действия:**
- [ ] Создать module
- [ ] `[DependsOn(typeof(CrmTenantsApplicationModule))]`
- [ ] `[DependsOn(typeof(CrmTenantsDataAccessModule))]`
- [ ] `[DependsOn(typeof(CrmAspNetCoreModule))]`
- [ ] Настроить middleware для tenant resolving

**Время:** 30 минут

---

## 1.7 Frontend Layer

### Задача 1.7.1: Создать Blazor компоненты

**Файлы:**
- `Pages/TenantList.razor`
- `Pages/TenantCreate.razor`
- `Pages/TenantEdit.razor`
- `Pages/TenantDetails.razor`
- `Components/TenantCard.razor`
- `Components/TenantSelector.razor`

**Действия:**
- [ ] Создать страницу списка тенантов
- [ ] Создать форму создания тенанта
- [ ] Создать форму редактирования
- [ ] Создать компонент для выбора тенанта (dropdown)
- [ ] Добавить навигацию

**Время:** 4 часа

### Задача 1.7.2: Создать Module класс

**Файл:** `CrmTenantsFrontendModule.cs`

**Действия:**
- [ ] Создать module
- [ ] `[DependsOn(typeof(CrmFrontendEventsModule))]`
- [ ] `[DependsOn(typeof(CrmFrontendCQRSModule))]`

**Время:** 15 минут

---

## 1.8 Testing

### Задача 1.8.1: Создать тестовый проект

**Структура:**
```
src/Cheetah.Tenants.Tests/
├── Domain/
├── Application/
├── Api/
└── Integration/
```

**Действия:**
- [ ] Создать Cheetah.Tenants.Tests проект
- [ ] Написать unit тесты для Domain (Tenant entity)
- [ ] Написать unit тесты для Commands/Queries
- [ ] Написать unit тесты для Services
- [ ] Написать integration тесты для API

**Время:** 6 часов

---

## 1.9 Documentation

### Задача 1.9.1: Создать README

**Файл:** `src/Cheetah.Tenants/README.md`

**Действия:**
- [ ] Описать модуль
- [ ] Добавить примеры использования
- [ ] Описать API endpoints
- [ ] Добавить диаграммы

**Время:** 1 час

---

## 1.10 Configuration

### Задача 1.10.1: Настроить appsettings

**Файл:** `src/Cheetah.Crm/appsettings.json`

**Действия:**
- [ ] Добавить ConnectionString для Tenants DB
- [ ] Добавить настройки для TenantResolver
- [ ] Добавить template для tenant databases

**Время:** 30 минут

---

**Итого Phase 1: ~25-30 часов работы**

---

# Phase 2: Cheetah.Identity Module

**Цель:** Создать систему управления пользователями и аутентификацию.

## 2.1 Domain Layer

### Задача 2.1.1: Создать Domain модели

**Файлы:**
- `Entities/User.cs`
- `Entities/UserTenant.cs`
- `Entities/UserRole.cs`
- `ValueObjects/Email.cs`

**User.cs:**
```csharp
public class User : AggregateRoot<Guid>, ICreateAtEntity
{
    public string Email { get; private set; }
    public string NormalizedEmail { get; private set; }
    public string PasswordHash { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public bool IsActive { get; private set; }
    public bool EmailConfirmed { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime CreatedAt { get; set; }

    public List<UserTenant> Tenants { get; private set; }

    public static User Create(string email, string passwordHash, ...);
    public void AddToTenant(Guid tenantId, List<Guid> roleIds);
    public void RemoveFromTenant(Guid tenantId);
    public void RecordLogin();
    public void ChangePassword(string newPasswordHash);
    public void ConfirmEmail();
}
```

**Действия:**
- [ ] Создать User aggregate
- [ ] Создать UserTenant (связь many-to-many с Tenant)
- [ ] Создать UserRole (связь с Role из Permissions)
- [ ] Создать Email value object с валидацией

**Время:** 2 часа

### Задача 2.1.2: Создать Domain Events

**Файлы:**
- `Events/UserCreatedEvent.cs`
- `Events/UserLoggedInEvent.cs`
- `Events/UserAddedToTenantEvent.cs`
- `Events/UserRemovedFromTenantEvent.cs`
- `Events/PasswordChangedEvent.cs`
- `Events/EmailConfirmedEvent.cs`

**Действия:**
- [ ] Создать все события
- [ ] Добавить необходимые данные

**Время:** 30 минут

---

## 2.2 Application Layer

### Задача 2.2.1: Создать Commands

**Файлы:**
- `Commands/RegisterUserCommand.cs` + Handler
- `Commands/LoginCommand.cs` + Handler
- `Commands/ChangePasswordCommand.cs` + Handler
- `Commands/ConfirmEmailCommand.cs` + Handler
- `Commands/AddUserToTenantCommand.cs` + Handler
- `Commands/RemoveUserFromTenantCommand.cs` + Handler
- `Commands/AssignRoleToUserCommand.cs` + Handler

**Действия:**
- [ ] Создать все команды
- [ ] Реализовать handlers
- [ ] Добавить валидацию

**Время:** 4 часа

### Задача 2.2.2: Создать Queries

**Файлы:**
- `Queries/GetUserByIdQuery.cs` + Handler
- `Queries/GetUserByEmailQuery.cs` + Handler
- `Queries/GetUserTenantsQuery.cs` + Handler
- `Queries/GetCurrentUserQuery.cs` + Handler

**Действия:**
- [ ] Создать запросы
- [ ] Реализовать handlers

**Время:** 2 часа

### Задача 2.2.3: Создать Services

**Файлы:**
- `Services/IAuthenticationService.cs`
- `Services/AuthenticationService.cs`
- `Services/IUserManager.cs`
- `Services/UserManager.cs`
- `Services/ITokenService.cs`
- `Services/TokenService.cs` (JWT)
- `Services/IPasswordHasher.cs`
- `Services/PasswordHasher.cs`
- `Services/ICurrentUser.cs`
- `Services/CurrentUser.cs`

**IAuthenticationService:**
```csharp
public interface IAuthenticationService
{
    Task<AuthenticationResult> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<AuthenticationResult> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
}

public class AuthenticationResult
{
    public bool Succeeded { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public List<string> Errors { get; set; }
}
```

**ITokenService:**
```csharp
public interface ITokenService
{
    string GenerateAccessToken(User user, Guid tenantId, List<string> permissions);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}
```

**Действия:**
- [ ] Реализовать AuthenticationService
- [ ] Реализовать UserManager
- [ ] Реализовать TokenService (JWT)
- [ ] Реализовать PasswordHasher (BCrypt или PBKDF2)
- [ ] Реализовать CurrentUser

**Время:** 5 часов

### Задача 2.2.4: Создать Middleware

**Файлы:**
- `Middleware/JwtAuthenticationMiddleware.cs`

**Действия:**
- [ ] Создать middleware для JWT аутентификации
- [ ] Интегрировать с ICurrentUser

**Время:** 1 час

---

## 2.3 DataAccess Layer

### Задача 2.3.1: Создать DbContext и Configurations

**Действия:**
- [ ] Создать IdentityDbContext
- [ ] Создать конфигурации для User, UserTenant, UserRole
- [ ] Создать migrations

**Время:** 2 часа

---

## 2.4 Shared Layer

### Задача 2.4.1: Создать ViewModels и Requests

**Файлы:**
- `ViewModels/UserViewModel.cs`
- `ViewModels/UserDetailsViewModel.cs`
- `Requests/LoginRequest.cs`
- `Requests/RegisterRequest.cs`
- `Requests/ChangePasswordRequest.cs`
- `Responses/AuthenticationResponse.cs`

**Время:** 1 час

---

## 2.5 Api Layer

### Задача 2.5.1: Создать Controllers

**Файлы:**
- `Controllers/AuthController.cs`
- `Controllers/UsersController.cs`

**Endpoints:**
- `POST /api/auth/login`
- `POST /api/auth/register`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `GET /api/users/me`
- `GET /api/users/{id}`
- `PUT /api/users/{id}`
- `POST /api/users/{id}/change-password`

**Время:** 2 часа

---

## 2.6 Frontend Layer

### Задача 2.6.1: Создать Blazor компоненты

**Файлы:**
- `Pages/Login.razor`
- `Pages/Register.razor`
- `Pages/UserProfile.razor`
- `Pages/UserList.razor`
- `Components/AuthStateProvider.cs`

**Действия:**
- [ ] Создать страницу логина
- [ ] Создать страницу регистрации
- [ ] Создать профиль пользователя
- [ ] Реализовать AuthenticationStateProvider
- [ ] Добавить хранение JWT токена

**Время:** 4 часа

---

## 2.7 Testing

### Задача 2.7.1: Создать тесты

**Действия:**
- [ ] Unit тесты для User entity
- [ ] Unit тесты для AuthenticationService
- [ ] Unit тесты для PasswordHasher
- [ ] Integration тесты для Auth API

**Время:** 4 часа

---

## 2.8 Event Handlers

### Задача 2.8.1: Подписаться на события Tenants

**Файл:** `EventHandlers/TenantCreatedEventHandler.cs`

**Действия:**
- [ ] При создании тенанта создавать admin пользователя
- [ ] Подписаться на TenantCreatedEvent

**Время:** 1 час

---

**Итого Phase 2: ~29-33 часа работы**

---

# Phase 3: Cheetah.Permissions Module

**Цель:** Создать систему прав доступа (RBAC).

## 3.1 Domain Layer

### Задача 3.1.1: Создать Domain модели

**Файлы:**
- `Entities/Permission.cs`
- `Entities/Role.cs`
- `Entities/RolePermission.cs`

**Permission.cs:**
```csharp
public class Permission : AggregateRoot<string> // Id = "Users.Create"
{
    public string Name { get; private set; }
    public string DisplayName { get; private set; }
    public string? Description { get; private set; }
    public string Group { get; private set; } // "Users", "Deals"
    public Guid TenantId { get; private set; }

    public static Permission Create(string name, string displayName, string group, Guid tenantId);
}
```

**Role.cs:**
```csharp
public class Role : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public string NormalizedName { get; private set; }
    public string? Description { get; private set; }
    public Guid TenantId { get; private set; }
    public bool IsSystemRole { get; private set; }

    public List<RolePermission> Permissions { get; private set; }

    public static Role Create(string name, Guid tenantId, bool isSystem = false);
    public void GrantPermission(string permissionId);
    public void RevokePermission(string permissionId);
}
```

**Действия:**
- [ ] Создать Permission entity (string Id)
- [ ] Создать Role aggregate
- [ ] Создать RolePermission (связь many-to-many)

**Время:** 1.5 часа

### Задача 3.1.2: Создать Domain Events

**Файлы:**
- `Events/RoleCreatedEvent.cs`
- `Events/PermissionGrantedEvent.cs`
- `Events/PermissionRevokedEvent.cs`

**Время:** 30 минут

---

## 3.2 Application Layer

### Задача 3.2.1: Создать Commands

**Файлы:**
- `Commands/CreateRoleCommand.cs` + Handler
- `Commands/UpdateRoleCommand.cs` + Handler
- `Commands/DeleteRoleCommand.cs` + Handler
- `Commands/GrantPermissionCommand.cs` + Handler
- `Commands/RevokePermissionCommand.cs` + Handler
- `Commands/CreatePermissionCommand.cs` + Handler

**Время:** 3 часа

### Задача 3.2.2: Создать Queries

**Файлы:**
- `Queries/GetRoleByIdQuery.cs` + Handler
- `Queries/GetAllRolesQuery.cs` + Handler
- `Queries/GetUserPermissionsQuery.cs` + Handler
- `Queries/GetAllPermissionsQuery.cs` + Handler
- `Queries/CheckPermissionQuery.cs` + Handler

**Время:** 2 часа

### Задача 3.2.3: Создать Services

**Файлы:**
- `Services/IPermissionChecker.cs`
- `Services/PermissionChecker.cs`
- `Services/IPermissionManager.cs`
- `Services/PermissionManager.cs`

**IPermissionChecker:**
```csharp
public interface IPermissionChecker
{
    Task<bool> IsGrantedAsync(string permissionName, CancellationToken ct = default);
    Task<bool> IsGrantedAsync(Guid userId, string permissionName, CancellationToken ct = default);
    Task RequireAsync(string permissionName, CancellationToken ct = default);
}
```

**Действия:**
- [ ] Реализовать PermissionChecker
- [ ] Реализовать PermissionManager
- [ ] Добавить кеширование прав пользователя

**Время:** 3 часа

### Задача 3.2.4: Создать Permission Definitions

**Файл:** `Permissions/SystemPermissions.cs`

```csharp
public static class SystemPermissions
{
    // Users
    public const string UsersCreate = "Users.Create";
    public const string UsersRead = "Users.Read";
    public const string UsersUpdate = "Users.Update";
    public const string UsersDelete = "Users.Delete";

    // Tenants
    public const string TenantsCreate = "Tenants.Create";
    // ... и т.д.
}
```

**Время:** 1 час

---

## 3.3 DataAccess Layer

**Время:** 2 часа

---

## 3.4 Shared Layer

**Время:** 1 час

---

## 3.5 Api Layer

### Задача 3.5.1: Создать Controllers

**Endpoints:**
- `GET /api/permissions` - все права
- `GET /api/roles` - все роли
- `POST /api/roles` - создать роль
- `PUT /api/roles/{id}` - обновить роль
- `DELETE /api/roles/{id}` - удалить роль
- `POST /api/roles/{id}/permissions` - добавить право
- `DELETE /api/roles/{id}/permissions/{permissionId}` - убрать право
- `GET /api/users/{userId}/permissions` - права пользователя

**Время:** 2 часа

---

## 3.6 Frontend Layer

### Задача 3.6.1: Создать Blazor компоненты

**Файлы:**
- `Pages/RoleList.razor`
- `Pages/RoleCreate.razor`
- `Pages/RoleEdit.razor`
- `Pages/PermissionMatrix.razor` - матрица прав для роли
- `Components/PermissionGuard.razor` - компонент для проверки прав

**Время:** 4 часа

---

## 3.7 Testing

**Время:** 3 часа

---

## 3.8 Event Handlers

### Задача 3.8.1: Подписаться на события

**EventHandlers:**
- `TenantCreatedEventHandler` - создать дефолтные роли (Admin, User)
- `UserCreatedEventHandler` - назначить роль User по умолчанию

**Время:** 1.5 часа

---

**Итого Phase 3: ~23-26 часов работы**

---

# Phase 4: Cheetah.Features Module

**Цель:** Создать систему feature flags для тенантов.

## 4.1 Domain Layer

### Задача 4.1.1: Создать Domain модели

**Файлы:**
- `Entities/Feature.cs`
- `Entities/TenantFeature.cs`

**Feature.cs:**
```csharp
public class Feature : AggregateRoot<string> // Id = "Users.Export"
{
    public string Name { get; private set; }
    public string DisplayName { get; private set; }
    public string? Description { get; private set; }
    public bool IsEnabledByDefault { get; private set; }
    public string? Group { get; private set; }

    public static Feature Create(string name, string displayName, bool enabledByDefault = false);
}
```

**TenantFeature.cs:**
```csharp
public class TenantFeature : Entity<Guid>
{
    public Guid TenantId { get; set; }
    public string FeatureId { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? EnabledAt { get; set; }
    public DateTime? DisabledAt { get; set; }
}
```

**Время:** 1 час

### Задача 4.1.2: Создать Domain Events

**Файлы:**
- `Events/FeatureEnabledEvent.cs`
- `Events/FeatureDisabledEvent.cs`

**Время:** 20 минут

---

## 4.2 Application Layer

### Задача 4.2.1: Создать Commands

**Файлы:**
- `Commands/CreateFeatureCommand.cs` + Handler
- `Commands/EnableFeatureCommand.cs` + Handler
- `Commands/DisableFeatureCommand.cs` + Handler

**Время:** 2 часа

### Задача 4.2.2: Создать Queries

**Файлы:**
- `Queries/GetAllFeaturesQuery.cs` + Handler
- `Queries/GetTenantFeaturesQuery.cs` + Handler
- `Queries/CheckFeatureQuery.cs` + Handler

**Время:** 1.5 часа

### Задача 4.2.3: Создать Services

**Файлы:**
- `Services/IFeatureChecker.cs`
- `Services/FeatureChecker.cs`
- `Services/IFeatureManager.cs`
- `Services/FeatureManager.cs`

**IFeatureChecker:**
```csharp
public interface IFeatureChecker
{
    Task<bool> IsEnabledAsync(string featureId, CancellationToken ct = default);
    Task<bool> IsEnabledAsync(string featureId, Guid tenantId, CancellationToken ct = default);
    Task RequireAsync(string featureId, CancellationToken ct = default);
}
```

**Действия:**
- [ ] Реализовать FeatureChecker с кешированием
- [ ] Реализовать FeatureManager

**Время:** 2 часа

### Задача 4.2.4: Создать Feature Definitions

**Файл:** `Features/SystemFeatures.cs`

```csharp
public static class SystemFeatures
{
    public const string UsersExport = "Users.Export";
    public const string ReportsAdvanced = "Reports.Advanced";
    public const string ApiAccess = "Api.Access";
    // ... и т.д.
}
```

**Время:** 30 минут

---

## 4.3 DataAccess Layer

**Время:** 1.5 часа

---

## 4.4 Shared Layer

**Время:** 45 минут

---

## 4.5 Api Layer

### Задача 4.5.1: Создать Controller

**Endpoints:**
- `GET /api/features` - все фичи
- `POST /api/features` - создать фичу
- `GET /api/tenants/{tenantId}/features` - фичи тенанта
- `POST /api/tenants/{tenantId}/features/{featureId}/enable`
- `POST /api/tenants/{tenantId}/features/{featureId}/disable`

**Время:** 1.5 часа

---

## 4.6 Frontend Layer

### Задача 4.6.1: Создать Blazor компоненты

**Файлы:**
- `Pages/FeatureList.razor`
- `Pages/TenantFeatureManagement.razor`
- `Components/FeatureGuard.razor` - компонент для проверки фич
- `Components/FeatureToggle.razor` - toggle для включения/выключения

**Время:** 3 часа

---

## 4.7 Testing

**Время:** 2 часа

---

## 4.8 Event Handlers

### Задача 4.8.1: Подписаться на события

**EventHandlers:**
- `TenantCreatedEventHandler` - включить базовые фичи для нового тенанта

**Время:** 1 час

---

**Итого Phase 4: ~16-18 часов работы**

---

# 🎯 Сводная таблица

| Phase | Модуль | Время (часы) | Приоритет |
|-------|--------|--------------|-----------|
| 1 | Tenants | 25-30 | 🔴 Критичный |
| 2 | Identity | 29-33 | 🔴 Критичный |
| 3 | Permissions | 23-26 | 🟡 Высокий |
| 4 | Features | 16-18 | 🟢 Средний |
| **ИТОГО** | | **93-107 часов** | |

---

# 📅 Рекомендуемый график

**При работе 8 часов в день:**
- Phase 1 (Tenants): 3-4 дня
- Phase 2 (Identity): 4-5 дней
- Phase 3 (Permissions): 3-4 дня
- Phase 4 (Features): 2-3 дня

**Общее время: 12-16 рабочих дней (2.5-3 недели)**

**При работе 4 часа в день:**
- Phase 1: 6-8 дней
- Phase 2: 7-9 дней
- Phase 3: 6-7 дней
- Phase 4: 4-5 дней

**Общее время: 23-29 рабочих дней (4.5-6 недель)**

---

# ✅ Чеклист перед началом

- [ ] Убедиться что Core модули работают корректно
- [ ] Настроить БД (SQL Server, PostgreSQL или MySQL)
- [ ] Настроить Redis для событий
- [ ] Подготовить тестовые данные
- [ ] Настроить CI/CD (опционально)
- [ ] Создать Git ветки для каждой фазы

---

# 🚀 Порядок работы (рекомендуемый)

1. **Phase 1: Tenants**
   - Начать с Domain → Application → DataAccess → Api → Shared → Frontend
   - Тестировать каждый слой перед переходом к следующему
   - Создать первый тенант и убедиться что резолвинг работает

2. **Phase 2: Identity**
   - Реализовать регистрацию и логин
   - Интегрировать с Tenants (связь User-Tenant)
   - Протестировать JWT аутентификацию

3. **Phase 3: Permissions**
   - Создать базовые роли и права
   - Интегрировать с Identity (UserRole)
   - Протестировать проверку прав

4. **Phase 4: Features**
   - Создать базовые фичи
   - Интегрировать с Tenants
   - Протестировать включение/выключение

---

# 🔄 Интеграционные точки между модулями

## Tenants → Identity
- User.Tenants (List<UserTenant>)
- TenantCreatedEvent → создание admin пользователя

## Identity → Permissions
- User.UserRoles (List<UserRole>)
- UserCreatedEvent → назначение дефолтной роли

## Tenants → Permissions
- Role.TenantId (каждая роль привязана к тенанту)
- Permission.TenantId (права могут быть tenant-specific)

## Tenants → Features
- TenantFeature.TenantId
- TenantCreatedEvent → включение базовых фич

---

# 📝 Примечания

1. **Тестирование:** После каждой фазы запускать все тесты
2. **Документация:** Обновлять README после завершения каждой фазы
3. **События:** Убедиться что Redis настроен корректно для межмодульной коммуникации
4. **Миграции:** Создавать отдельные миграции для каждого модуля
5. **API:** Тестировать endpoints через Swagger/Postman
6. **Frontend:** Тестировать в Blazor WASM после каждого модуля

---

# 🎓 Рекомендации

1. **Начни с малого:** Реализуй минимальный функционал каждого слоя, потом расширяй
2. **Тестируй часто:** Не накапливай непротестированный код
3. **Коммить регулярно:** После каждой завершенной задачи
4. **Используй ветки:** Создай ветку для каждой фазы
5. **Документируй:** Пиши комментарии к сложным местам
6. **Переиспользуй код:** Ищи паттерны, создавай базовые классы
7. **Следуй соглашениям:** Придерживайся naming conventions из Claude.md

Удачи в реализации! 🚀
