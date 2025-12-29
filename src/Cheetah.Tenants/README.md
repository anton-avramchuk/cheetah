# Cheetah.Tenants Module

## Описание

Модуль управления тенантами (мультитенантность) для Cheetah CRM. Реализует tenant-per-database подход с автоматическим резолвингом тенанта через subdomain или header.

## Архитектура модуля

```
Cheetah.Tenants/
├── Cheetah.Tenants.Domain/          # Entities, Events, Value Objects
├── Cheetah.Tenants.Application/     # CQRS, Services, Business Logic
├── Cheetah.Tenants.DataAccess/      # EF Core, DbContext, Migrations
├── Cheetah.Tenants.Api/             # Minimal API Endpoints
├── Cheetah.Tenants.Shared/          # DTOs, ViewModels (shared)
├── Cheetah.Tenants.Client/          # Client для Backend модулей
├── Cheetah.Tenants.ApiClient/       # HTTP Client для Frontend
└── Cheetah.Tenants.Frontend/        # Blazor WASM UI компоненты
```

## Разделение Client модулей

### Cheetah.Tenants.Client (для Backend)
**Использование:** Другие backend модули (Identity, Permissions, Features)
**Технология:** IDispatcher + ITenantStore (прямой доступ)
**Интерфейс:** `ITenantClientService`

```csharp
// В модуле Identity
[DependsOn(typeof(CrmTenantsClientModule))]
public partial class CrmIdentityApplicationModule : CrmModule { }

// В сервисе
public class UserService
{
    private readonly ITenantClientService _tenantClient;

    public async Task ValidateTenant(Guid tenantId)
    {
        if (!await _tenantClient.IsActiveAsync(tenantId))
            throw new Exception("Tenant not active");
    }
}
```

### Cheetah.Tenants.ApiClient (для Frontend)
**Использование:** Blazor WASM Frontend
**Технология:** HttpClient (REST API)
**Интерфейс:** `ITenantApiClient`

```razor
@inject ITenantApiClient TenantApiClient

@code {
    protected override async Task OnInitializedAsync()
    {
        tenants = await TenantApiClient.GetAllAsync();
    }
}
```

## Компоненты модуля

### 1. Domain Layer
**Путь:** `Cheetah.Tenants.Domain`

**Entities:**
- `Tenant` - основная сущность тенанта
- `TenantConnectionString` - строки подключения для БД тенанта

**Events:**
- `TenantCreatedEvent` - тенант создан
- `TenantActivatedEvent` - тенант активирован
- `TenantDeactivatedEvent` - тенант деактивирован

**Value Objects:**
- `Subdomain` - поддомен тенанта

### 2. Application Layer
**Путь:** `Cheetah.Tenants.Application`

**Commands:**
- `CreateTenantCommand` - создать тенант
- `ActivateTenantCommand` - активировать
- `DeactivateTenantCommand` - деактивировать

**Queries:**
- `GetTenantByIdQuery` - получить по ID
- `GetTenantBySubdomainQuery` - получить по поддомену
- `GetAllTenantsQuery` - получить все

**Services:**
- `ITenantResolver` - резолвинг тенанта из HTTP контекста
- `ICurrentTenant` - текущий тенант (scoped)
- `ITenantStore` - доступ к данным тенантов
- `TenantResolverMiddleware` - middleware для резолвинга

### 3. DataAccess Layer
**Путь:** `Cheetah.Tenants.DataAccess`

**DbContext:**
- `TenantsDbContext` - контекст для master базы с информацией о всех тенантах

**Configurations:**
- `TenantConfiguration` - конфигурация Tenant entity
- `TenantConnectionStringConfiguration` - конфигурация связанных строк подключения

### 4. API Layer
**Путь:** `Cheetah.Tenants.Api`

**Endpoints:**
- `GET /api/tenants` - список всех тенантов
- `GET /api/tenants/{id}` - получить тенант
- `POST /api/tenants` - создать тенант
- `POST /api/tenants/{id}/activate` - активировать
- `POST /api/tenants/{id}/deactivate` - деактивировать

### 5. Shared Layer
**Путь:** `Cheetah.Tenants.Shared`

**ViewModels:**
- `TenantViewModel` - для отображения
- `TenantConnectionStringViewModel` - connection string

**Requests:**
- `CreateTenantRequest` - создание
- `ActivateTenantRequest` - активация
- `DeactivateTenantRequest` - деактивация

### 6. Client Layer (Backend)
**Путь:** `Cheetah.Tenants.Client`

**Interface:** `ITenantClientService`

**Методы:**
- `GetByIdAsync` - получить по ID
- `GetBySubdomainAsync` - получить по поддомену
- `GetAllActiveAsync` - все активные
- `GetConnectionStringAsync` - строка подключения
- `IsActiveAsync` - проверка активности
- `ExistsAsync` - проверка существования

### 7. ApiClient Layer (Frontend)
**Путь:** `Cheetah.Tenants.ApiClient`

**Interface:** `ITenantApiClient`

**Методы:**
- `GetAllAsync` - список тенантов
- `GetByIdAsync` - получить по ID
- `CreateAsync` - создать
- `ActivateAsync` - активировать
- `DeactivateAsync` - деактивировать

### 8. Frontend Layer
**Путь:** `Cheetah.Tenants.Frontend`

**Pages:**
- `TenantList` (`/tenants`) - список
- `TenantCreate` (`/tenants/create`) - создание
- `TenantDetails` (`/tenants/{id}`) - детали
- `TenantEdit` (`/tenants/{id}/edit`) - редактирование (TODO)

**Components:**
- `TenantCard` - карточка тенанта

## Основные концепции

### Tenant-per-Database
Каждый тенант имеет свою собственную базу данных:

```
Master DB (TenantsDb)
├── Tenant #1 → ConnectionString → Tenant1_DB
├── Tenant #2 → ConnectionString → Tenant2_DB
└── Tenant #3 → ConnectionString → Tenant3_DB
```

### Tenant Resolving
Тенант определяется автоматически через middleware:

1. **HTTP Header:** `X-Tenant-Id` или `X-Tenant-Subdomain`
2. **Subdomain:** `acme.yourapp.com` → tenant с subdomain = "acme"

```csharp
// В любом месте приложения
public class MyService
{
    private readonly ICurrentTenant _currentTenant;

    public void DoSomething()
    {
        var tenantId = _currentTenant.Id; // Автоматически резолвится
        var tenantName = _currentTenant.Name;
    }
}
```

### Event-Driven Integration
Другие модули подписываются на события тенантов:

```csharp
// В модуле Identity при создании тенанта создаем admin пользователя
[Export(LifetimeType.Scoped)]
public class TenantCreatedEventHandler : IEventHandler<TenantCreatedEvent>
{
    public async ValueTask HandleAsync(TenantCreatedEvent @event, CancellationToken ct)
    {
        // Создать admin пользователя для нового тенанта
        var admin = User.Create("admin@example.com", @event.TenantId);
        // ...
    }
}
```

## Использование в других модулях

### Backend модуль (например, Identity)

```csharp
// 1. Добавить зависимость
[DependsOn(typeof(CrmTenantsClientModule))]
public partial class CrmIdentityApplicationModule : CrmModule { }

// 2. Использовать ITenantClientService
public class UserCommandHandler
{
    private readonly ITenantClientService _tenantClient;

    public async Task HandleAsync(CreateUserCommand cmd, CancellationToken ct)
    {
        // Проверить тенант
        if (!await _tenantClient.ExistsAsync(cmd.TenantId, ct))
            throw new Exception("Tenant not found");

        // Создать пользователя...
    }
}
```

### Frontend модуль (Blazor WASM)

```csharp
// 1. Добавить зависимость
[DependsOn(typeof(CrmTenantsFrontendModule))]
public partial class MyFrontendModule : CrmModule { }

// 2. Использовать в Razor компонентах
@inject ITenantApiClient TenantApiClient

<select @bind="selectedTenantId">
    @foreach (var tenant in tenants)
    {
        <option value="@tenant.Id">@tenant.Name</option>
    }
</select>

@code {
    private List<TenantViewModel> tenants = new();

    protected override async Task OnInitializedAsync()
    {
        tenants = await TenantApiClient.GetAllAsync();
    }
}
```

## Конфигурация

### appsettings.json

```json
{
  "ConnectionStrings": {
    "TenantsDb": "Server=localhost;Database=Cheetah_Tenants;..."
  },
  "TenantResolver": {
    "DefaultTenantId": "00000000-0000-0000-0000-000000000000",
    "EnableSubdomainResolving": true,
    "EnableHeaderResolving": true
  }
}
```

### Регистрация в Program.cs

```csharp
// Backend API
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCheetahModules<CrmTenantsApiModule>(builder.Configuration);

var app = builder.Build();
app.UseCheetahModules();
app.Run();
```

## База данных

### Master DB Schema

```sql
CREATE TABLE Tenants (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    NormalizedName NVARCHAR(200) NOT NULL UNIQUE,
    Subdomain NVARCHAR(100) UNIQUE,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIMEOFFSET NOT NULL,
    UpdatedAt DATETIMEOFFSET NOT NULL
);

CREATE TABLE TenantConnectionStrings (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    ConnectionString NVARCHAR(MAX) NOT NULL,
    IsDefault BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
);
```

### Migrations

```bash
# Создать миграцию
dotnet ef migrations add InitialCreate --project src/Cheetah.Tenants/Cheetah.Tenants.DataAccess

# Применить миграцию
dotnet ef database update --project src/Cheetah.Tenants/Cheetah.Tenants.DataAccess
```

## Тестирование

### Unit тесты
- Domain logic (Tenant entity methods)
- Commands/Queries handlers
- Services (TenantResolver, TenantStore)

### Integration тесты
- API endpoints
- Database operations
- Event publishing

## Статус реализации

| Компонент | Статус | Примечания |
|-----------|--------|------------|
| Domain | ✅ Complete | Entities, Events |
| Application | ✅ Complete | Commands, Queries, Services |
| DataAccess | ✅ Complete | DbContext, Configurations |
| API | ✅ Complete | CRUD endpoints (без Update) |
| Shared | ✅ Complete | ViewModels, Requests |
| Client | ✅ Complete | Backend client service |
| ApiClient | ✅ Complete | Frontend HTTP client |
| Frontend | ✅ Complete | CRUD UI (Edit - placeholder) |

## TODO

- [ ] Добавить Update endpoint в API
- [ ] Реализовать полное редактирование в Frontend
- [ ] Добавить тесты для всех слоев
- [ ] Добавить документацию API (Swagger/OpenAPI)
- [ ] Реализовать управление Connection Strings через UI
- [ ] Добавить аудит действий с тенантами

## Дополнительные ресурсы

- [Client README](./Cheetah.Tenants.Client/README.md) - Backend клиент
- [ApiClient README](./Cheetah.Tenants.ApiClient/README.md) - Frontend HTTP клиент
- [Frontend README](./Cheetah.Tenants.Frontend/README.md) - Blazor UI компоненты
