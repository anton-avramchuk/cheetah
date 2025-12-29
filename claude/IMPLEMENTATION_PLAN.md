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
├── Cheetah.Tenants.Client/              # Backend client (IDispatcher)
├── Cheetah.Tenants.ApiClient/           # Frontend client (HTTP)
├── Cheetah.Tenants.Frontend/
├── Cheetah.Tenants.Client.Tests/        # Тесты для Client
└── Cheetah.Tenants.ApiClient.Tests/     # Тесты для ApiClient
```

**Действия:**
- [ ] Создать папку `src/Cheetah.Tenants/`
- [ ] Создать 10 подпапок для каждого слоя
- [ ] Создать `.csproj` файлы для каждого проекта
- [ ] Добавить проекты в `Cheetah.slnx`

**Время:** 45 минут

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
- [ ] `[DependsOn(typeof(CrmEntityFrameworkModule))]`
- [ ] `[DependsOn(typeof(CrmEntityFrameworkPostgreSqlModule))]`

**Пример:**
```csharp
[DependsOn(typeof(CrmTenantsDomainModule))]
[DependsOn(typeof(CrmEntityFrameworkModule))]
[DependsOn(typeof(CrmEntityFrameworkPostgreSqlModule))]
public partial class CrmTenantsDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.AddDbContext<TenantsDbContext>(options =>
        {
            var connectionString = context.Services.GetConfiguration()
                .GetConnectionString("Tenants");
            options.UseNpgsql(connectionString);
        });
    }
}
```

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

## 1.6 Client Libraries

### Задача 1.6.1: Создать Backend Client (Cheetah.Tenants.Client)

**Файл:** `Implementation/TenantClientService.cs`

**Интерфейс:**
```csharp
public interface ITenantClientService
{
    Task<TenantViewModel?> GetByIdAsync(Guid tenantId, CancellationToken ct = default);
    Task<TenantViewModel?> GetBySubdomainAsync(string subdomain, CancellationToken ct = default);
    Task<IReadOnlyList<TenantViewModel>> GetAllActiveAsync(CancellationToken ct = default);
    Task<string?> GetConnectionStringAsync(Guid tenantId, string name = "Default", CancellationToken ct = default);
    Task<bool> IsActiveAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid tenantId, CancellationToken ct = default);
}
```

**Реализация:**
```csharp
[Export(LifetimeType.Scoped, typeof(ITenantClientService))]
public class TenantClientService : ITenantClientService
{
    private readonly IDispatcher _dispatcher;
    private readonly ITenantStore _tenantStore;

    public async Task<TenantViewModel?> GetByIdAsync(Guid tenantId, CancellationToken ct = default)
    {
        var query = new GetTenantByIdQuery(tenantId);
        var tenant = await _dispatcher.QueryAsync<GetTenantByIdQuery, Tenant?>(query, ct);
        return tenant != null ? new TenantViewModel { Id = tenant.Id, Name = tenant.Name } : null;
    }
    // ... остальные методы
}
```

**Действия:**
- [ ] Создать интерфейс ITenantClientService
- [ ] Создать реализацию TenantClientService с [Export] атрибутом
- [ ] Использовать IDispatcher для запросов
- [ ] Использовать ITenantStore для connection strings
- [ ] Создать Module класс с зависимостью от Application

**Время:** 2 часа

### Задача 1.6.2: Создать Frontend Client (Cheetah.Tenants.ApiClient)

**Файл:** `Implementation/TenantApiClient.cs`

**Интерфейс:**
```csharp
public interface ITenantApiClient
{
    Task<IReadOnlyList<TenantViewModel>> GetAllAsync(CancellationToken ct = default);
    Task<TenantViewModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TenantViewModel> CreateAsync(CreateTenantRequest request, CancellationToken ct = default);
    Task<TenantViewModel> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken ct = default);
    Task ActivateAsync(Guid id, CancellationToken ct = default);
    Task DeactivateAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

**Реализация:**
```csharp
[Export(LifetimeType.Scoped, typeof(ITenantApiClient))]
public class TenantApiClient : ITenantApiClient
{
    private readonly HttpClient _httpClient;

    public async Task<IReadOnlyList<TenantViewModel>> GetAllAsync(CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync("/api/tenants", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<TenantViewModel>>(ct)
            ?? new List<TenantViewModel>();
    }
    // ... остальные методы
}
```

**Действия:**
- [ ] Создать интерфейс ITenantApiClient
- [ ] Создать реализацию TenantApiClient с [Export] атрибутом
- [ ] Использовать HttpClient для HTTP запросов
- [ ] Обрабатывать HTTP статусы и ошибки
- [ ] Создать Module класс с зависимостью от Shared

**Время:** 2 часа

### Задача 1.6.3: Создать тесты для Client

**Файл:** `Cheetah.Tenants.Client.Tests/Implementation/TenantClientServiceTests.cs`

**Тесты:**
- [ ] GetByIdAsync - успешный кейс
- [ ] GetByIdAsync - тенант не найден
- [ ] GetBySubdomainAsync - успешный кейс
- [ ] GetBySubdomainAsync - тенант не найден
- [ ] GetAllActiveAsync - фильтрация активных
- [ ] GetConnectionStringAsync - успешный кейс
- [ ] GetConnectionStringAsync - тенант/connection string не найден
- [ ] IsActiveAsync - активный/неактивный
- [ ] ExistsAsync - существует/не существует
- [ ] Проверка передачи CancellationToken

**Технологии:**
- xUnit для тестов
- Moq для мока IDispatcher и ITenantStore
- FluentAssertions для проверок

**Время:** 2.5 часа

### Задача 1.6.4: Создать тесты для ApiClient

**Файл:** `Cheetah.Tenants.ApiClient.Tests/Implementation/TenantApiClientTests.cs`

**Тесты:**
- [ ] GetAllAsync - успешный кейс
- [ ] GetAllAsync - пустой список
- [ ] GetByIdAsync - успешный кейс
- [ ] GetByIdAsync - 404 Not Found
- [ ] CreateAsync - успешный кейс
- [ ] CreateAsync - 400 Bad Request
- [ ] UpdateAsync, ActivateAsync, DeactivateAsync, DeleteAsync
- [ ] Проверка правильности HTTP методов и URL
- [ ] Проверка передачи CancellationToken

**Технологии:**
- xUnit для тестов
- HttpMessageHandler mock для мока HTTP запросов
- FluentAssertions для проверок

**Время:** 2.5 часа

---

## 1.7 Api Layer

### Задача 1.7.1: Создать Minimal API Endpoints

**Файл:** `CrmTenantsApiModule.cs` (OnApplicationInitialization method)

**Endpoints:**
- `POST /api/tenants` - создать тенант
- `GET /api/tenants/{id}` - получить тенант
- `GET /api/tenants` - получить все тенанты
- `PUT /api/tenants/{id}` - обновить тенант
- `POST /api/tenants/{id}/activate` - активировать
- `POST /api/tenants/{id}/deactivate` - деактивировать
- `DELETE /api/tenants/{id}` - удалить (soft delete)

**Действия:**
- [ ] Создать все endpoints в OnApplicationInitialization
- [ ] Использовать IDispatcher для обработки запросов
- [ ] Использовать IObjectMapper (Mapster) для маппинга Request → Command
- [ ] Добавить `.WithName()` и `.WithOpenApi()` для каждого endpoint
- [ ] Обрабатывать ошибки и возвращать правильные HTTP статусы

**Время:** 3 часа

### Задача 1.7.2: Создать Module класс

**Файл:** `CrmTenantsApiModule.cs`

**Действия:**
- [ ] Создать module
- [ ] `[DependsOn(typeof(CrmTenantsApplicationModule))]`
- [ ] `[DependsOn(typeof(CrmTenantsDataAccessModule))]`
- [ ] `[DependsOn(typeof(CrmMapsterModule))]`
- [ ] `[DependsOn(typeof(CrmAspNetCoreModule))]`
- [ ] Настроить middleware для tenant resolving
- [ ] Реализовать все endpoints в OnApplicationInitialization

**Время:** 30 минут

---

## 1.8 Frontend Layer (Blazor WASM - полноценный CRUD)

### Задача 1.8.1: Создать страницу списка (TenantList.razor)

**Файл:** `Pages/TenantList.razor`

**Компоненты из Cheetah.Blazor.Components:**
- `CrmDataGrid<TenantViewModel>` - таблица с данными
- `CrmButton` - кнопки действий
- `CrmBadge` - статус (активный/неактивный)
- `CrmLoadingSpinner` - индикатор загрузки
- `CrmPagination` - пагинация
- `CrmAlert` - сообщения об ошибках

**Функционал:**
- [ ] Отображение списка тенантов в CrmDataGrid
- [ ] Столбцы: Name, Subdomain, Status, Actions
- [ ] Кнопка "Создать тенант" → навигация на TenantCreate
- [ ] Кнопки действий: Edit, Activate/Deactivate, Delete
- [ ] Поиск/фильтрация по имени
- [ ] Пагинация через CrmPagination
- [ ] Обработка ошибок с CrmAlert

**Время:** 3 часа

### Задача 1.8.2: Создать страницу создания (TenantCreate.razor)

**Файл:** `Pages/TenantCreate.razor`

**Компоненты из Cheetah.Blazor.Components:**
- `CrmCard` - обертка формы
- `CrmTextInput` - поля ввода
- `CrmButton` - кнопки Save/Cancel
- `CrmAlert` - валидационные ошибки

**Функционал:**
- [ ] Форма с полями: Name, Subdomain
- [ ] Валидация на клиенте (required, length)
- [ ] Кнопка "Save" - вызов ITenantApiClient.CreateAsync
- [ ] Кнопка "Cancel" - навигация назад
- [ ] Отображение ошибок валидации
- [ ] После успешного создания → редирект на список

**Время:** 2 часа

### Задача 1.8.3: Создать страницу редактирования (TenantEdit.razor)

**Файл:** `Pages/TenantEdit.razor`

**Компоненты из Cheetah.Blazor.Components:**
- `CrmCard` - обертка формы
- `CrmTextInput` - поля ввода
- `CrmButton` - кнопки Save/Cancel/Delete
- `CrmAlert` - ошибки
- `CrmLoadingSpinner` - загрузка данных

**Функционал:**
- [ ] Загрузка тенанта по ID через ITenantApiClient.GetByIdAsync
- [ ] Форма с полями: Name, Subdomain
- [ ] Кнопка "Save" - вызов ITenantApiClient.UpdateAsync
- [ ] Кнопка "Activate/Deactivate"
- [ ] Кнопка "Delete" с подтверждением
- [ ] Отображение ошибок
- [ ] После успешного обновления → редирект на список

**Время:** 2.5 часа

### Задача 1.8.4: Создать компонент удаления (DeleteTenantDialog)

**Файл:** `Components/DeleteTenantDialog.razor`

**Компоненты из Cheetah.Blazor.Components:**
- `CrmCard` - диалог
- `CrmButton` - кнопки Confirm/Cancel
- `CrmAlert` - предупреждение

**Функционал:**
- [ ] Модальное окно подтверждения удаления
- [ ] Отображение имени удаляемого тенанта
- [ ] Кнопка "Confirm" - вызов ITenantApiClient.DeleteAsync
- [ ] Кнопка "Cancel" - закрытие диалога

**Время:** 1 час

### Задача 1.8.5: Создать дополнительные компоненты

**Файлы:**
- `Components/TenantSelector.razor` - dropdown для выбора тенанта
- `Components/TenantStatusBadge.razor` - бейдж статуса

**Компоненты из Cheetah.Blazor.Components:**
- `CrmBadge` - для статуса

**Время:** 1.5 часа

### Задача 1.8.6: Создать Module класс

**Файл:** `CrmTenantsFrontendModule.cs`

**Действия:**
- [ ] Создать module
- [ ] `[DependsOn(typeof(CrmFrontendEventsModule))]`
- [ ] `[DependsOn(typeof(CrmFrontendCQRSModule))]`
- [ ] `[DependsOn(typeof(CrmTenantsApiClientModule))]`
- [ ] `[DependsOn(typeof(CrmBlazorComponentsModule))]`
- [ ] Настроить навигацию в меню

**Время:** 30 минут

---

## 1.9 Testing (Domain & Application)

### Задача 1.9.1: Создать тестовый проект

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

**Примечание:** Тесты для Client и ApiClient уже созданы в задачах 1.6.3 и 1.6.4

**Время:** 6 часов

---

## 1.10 Documentation

### Задача 1.10.1: Создать README

**Файл:** `src/Cheetah.Tenants/README.md`

**Действия:**
- [ ] Описать модуль
- [ ] Добавить примеры использования клиентских библиотек
- [ ] Описать API endpoints
- [ ] Добавить диаграммы
- [ ] Описать Blazor компоненты

**Время:** 1.5 часа

---

## 1.11 Configuration

### Задача 1.11.1: Настроить appsettings

**Файл:** `src/Cheetah.Crm/appsettings.json`

**Действия:**
- [ ] Добавить ConnectionString для Tenants DB (PostgreSQL)
- [ ] Добавить настройки для TenantResolver
- [ ] Добавить template для tenant databases

**Пример:**
```json
{
  "ConnectionStrings": {
    "Tenants": "Host=localhost;Database=CheetahTenants;Username=postgres;Password=***"
  },
  "TenantResolver": {
    "DefaultSubdomain": "default",
    "SubdomainPattern": "^[a-z0-9-]+$"
  }
}
```

**Время:** 30 минут

---

**Итого Phase 1: ~39-44 часа работы** (с учетом клиентских библиотек, тестов и полноценного CRUD)

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

**Итого Phase 2: ~43-48 часов работы** (с учетом клиентских библиотек, тестов и полноценного CRUD)

**Примечание:** Для Phase 2 также требуется:
- Создать Cheetah.Identity.Client (backend client с IDispatcher)
- Создать Cheetah.Identity.ApiClient (frontend client с HttpClient)
- Создать тесты для обеих клиентских библиотек
- Реализовать полноценный CRUD в Blazor с использованием Cheetah.Blazor.Components
- Использовать CrmEntityFrameworkModule + CrmEntityFrameworkPostgreSqlModule
- Использовать Minimal API вместо Controllers
- Использовать CrmMapsterModule для маппинга

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

**Итого Phase 3: ~37-41 час работы** (с учетом клиентских библиотек, тестов и полноценного CRUD)

**Примечание:** Для Phase 3 также требуется:
- Создать Cheetah.Permissions.Client (backend client с IDispatcher)
- Создать Cheetah.Permissions.ApiClient (frontend client с HttpClient)
- Создать тесты для обеих клиентских библиотек
- Реализовать полноценный CRUD в Blazor с использованием Cheetah.Blazor.Components
- Использовать CrmEntityFrameworkModule + CrmEntityFrameworkPostgreSqlModule
- Использовать Minimal API вместо Controllers
- Использовать CrmMapsterModule для маппинга

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

**Итого Phase 4: ~30-33 часа работы** (с учетом клиентских библиотек, тестов и полноценного CRUD)

**Примечание:** Для Phase 4 также требуется:
- Создать Cheetah.Features.Client (backend client с IDispatcher)
- Создать Cheetah.Features.ApiClient (frontend client с HttpClient)
- Создать тесты для обеих клиентских библиотек
- Реализовать полноценный CRUD в Blazor с использованием Cheetah.Blazor.Components
- Использовать CrmEntityFrameworkModule + CrmEntityFrameworkPostgreSqlModule
- Использовать Minimal API вместо Controllers
- Использовать CrmMapsterModule для маппинга

---

# 🎯 Сводная таблица

| Phase | Модуль | Время (часы) | Приоритет | Изменения |
|-------|--------|--------------|-----------|-----------|
| 1 | Tenants | 39-44 | 🔴 Критичный | +2 Client libs, +2 Tests, CRUD UI |
| 2 | Identity | 43-48 | 🔴 Критичный | +2 Client libs, +2 Tests, CRUD UI |
| 3 | Permissions | 37-41 | 🟡 Высокий | +2 Client libs, +2 Tests, CRUD UI |
| 4 | Features | 30-33 | 🟢 Средний | +2 Client libs, +2 Tests, CRUD UI |
| **ИТОГО** | | **149-166 часов** | | **(было 93-107 часов)** |

**Увеличение времени обусловлено:**
- Добавлением 2 клиентских библиотек на каждый модуль (Client + ApiClient)
- Написанием тестов для каждой клиентской библиотеки
- Полноценным CRUD UI в Blazor с использованием Cheetah.Blazor.Components
- Использованием Minimal API (требует больше времени на настройку)
- Использованием PostgreSQL + Mapster

---

# 📅 Рекомендуемый график

**При работе 8 часов в день:**
- Phase 1 (Tenants): 5-6 дней
- Phase 2 (Identity): 5-6 дней
- Phase 3 (Permissions): 5-6 дней
- Phase 4 (Features): 4-5 дней

**Общее время: 19-23 рабочих дня (~4-4.5 недели)**

**При работе 4 часа в день:**
- Phase 1: 10-11 дней
- Phase 2: 11-12 дней
- Phase 3: 9-11 дней
- Phase 4: 8-9 дней

**Общее время: 38-43 рабочих дня (~7-8.5 недель)**

---

# ✅ Чеклист перед началом

- [ ] Убедиться что Core модули работают корректно
- [ ] Установить и настроить PostgreSQL
- [ ] Установить и настроить Redis для событий (backend)
- [ ] Убедиться что CrmEntityFrameworkModule и CrmEntityFrameworkPostgreSqlModule работают
- [ ] Убедиться что CrmMapsterModule настроен
- [ ] Убедиться что Cheetah.Blazor.Components готовы к использованию
- [ ] Подготовить тестовые данные
- [ ] Настроить CI/CD (опционально)
- [ ] Создать Git ветки для каждой фазы

---

# 🚀 Порядок работы (рекомендуемый)

1. **Phase 1: Tenants**
   - Порядок разработки: Domain → Application → DataAccess (PostgreSQL) → Shared → Client (backend) → ApiClient (frontend) → Api (Minimal API) → Frontend (Blazor CRUD)
   - Сразу после Client/ApiClient писать тесты для них
   - Тестировать каждый слой перед переходом к следующему
   - Создать первый тенант через API и убедиться что резолвинг работает
   - Протестировать CRUD UI в Blazor

2. **Phase 2: Identity**
   - Аналогичный порядок разработки
   - Реализовать регистрацию и логин
   - Создать Client и ApiClient
   - Интегрировать с Tenants (связь User-Tenant)
   - Протестировать JWT аутентификацию
   - Протестировать CRUD UI

3. **Phase 3: Permissions**
   - Аналогичный порядок разработки
   - Создать базовые роли и права
   - Создать Client и ApiClient
   - Интегрировать с Identity (UserRole)
   - Протестировать проверку прав
   - Протестировать CRUD UI

4. **Phase 4: Features**
   - Аналогичный порядок разработки
   - Создать базовые фичи
   - Создать Client и ApiClient
   - Интегрировать с Tenants
   - Протестировать включение/выключение
   - Протестировать CRUD UI

**Важно:** На каждом этапе использовать Cheetah.Blazor.Components для UI, PostgreSQL для БД, и Minimal API вместо Controllers.

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

1. **Тестирование:**
   - После каждой фазы запускать все тесты
   - Обязательно тестировать Client и ApiClient
   - Тестировать CRUD UI в Blazor

2. **Документация:**
   - Обновлять README после завершения каждой фазы
   - Документировать клиентские библиотеки с примерами использования

3. **События:**
   - Убедиться что Redis настроен корректно для межмодульной коммуникации (backend)
   - Frontend использует In-Memory Event Bus

4. **Миграции:**
   - Создавать отдельные миграции для каждого модуля
   - Использовать PostgreSQL вместо SQL Server

5. **API:**
   - Использовать ТОЛЬКО Minimal API (не Controllers)
   - Регистрировать endpoints в OnApplicationInitialization
   - Использовать Mapster для маппинга Request → Command
   - Тестировать endpoints через Swagger/Postman

6. **Frontend:**
   - Использовать Cheetah.Blazor.Components для всех UI элементов
   - Реализовать полноценный CRUD (Create, Read, Update, Delete)
   - Использовать ApiClient для HTTP запросов
   - Тестировать в Blazor WASM после каждого модуля

7. **Клиентские библиотеки:**
   - Client (backend) использует IDispatcher для CQRS
   - ApiClient (frontend) использует HttpClient для HTTP запросов
   - Обе библиотеки должны иметь comprehensive тесты

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

---

# 🔧 Технологический стек

## Backend
- **.NET 10.0** - основной фреймворк
- **ASP.NET Core** - веб-фреймворк
- **Minimal API** - для endpoints (вместо Controllers)
- **Entity Framework Core** - ORM
- **PostgreSQL** - база данных
- **Redis** - для event bus (межмодульная коммуникация)
- **Mapster** - object mapping
- **xUnit** - тестирование
- **Moq** - моки для тестов
- **FluentAssertions** - assertion библиотека

## Frontend
- **Blazor WebAssembly** - клиентский фреймворк
- **Cheetah.Blazor.Components** - библиотека UI компонентов
- **HttpClient** - для API запросов
- **In-Memory Event Bus** - для frontend событий

## Архитектурные паттерны
- **CQRS** - разделение команд и запросов
- **DDD** - domain-driven design
- **Event-Driven Architecture** - межмодульная коммуникация через события
- **Repository Pattern** - доступ к данным
- **Dependency Injection** - через Source Generators

## Ключевые модули
- **CrmEntityFrameworkModule** - базовый модуль для EF Core
- **CrmEntityFrameworkPostgreSqlModule** - PostgreSQL провайдер
- **CrmMapsterModule** - маппинг объектов
- **CrmBlazorComponentsModule** - UI компоненты
- **CrmBackendCQRSModule** - CQRS для backend
- **CrmFrontendCQRSModule** - CQRS для frontend
- **CrmBackendEventsModule** - события для backend (Redis)
- **CrmFrontendEventsModule** - события для frontend (In-Memory)
