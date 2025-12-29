# Cheetah.Tenants.Client

## Описание

Модуль-клиент для доступа к функциональности Tenants из других модулей. Предоставляет абстракцию `ITenantClientService`, которая позволяет легко переключаться между прямым доступом к данным и HTTP-клиентом при переходе на микросервисную архитектуру.

## Архитектура

```
┌─────────────────────────────┐
│  Identity / Permissions /   │
│  Features / Other Modules   │
└──────────┬──────────────────┘
           │ uses
           ▼
┌─────────────────────────────┐
│  ITenantClientService       │  ◄─── Абстракция
└──────────┬──────────────────┘
           │ implements
           ▼
┌─────────────────────────────┐
│  TenantClientService        │  ◄─── Текущая реализация
│  (uses IDispatcher +        │       (прямой доступ)
│   ITenantStore)             │
└─────────────────────────────┘

           │ в будущем заменится на
           ▼
┌─────────────────────────────┐
│  TenantHttpClientService    │  ◄─── Будущая реализация
│  (uses HttpClient)          │       (HTTP для микросервисов)
└─────────────────────────────┘
```

## Использование

### 1. Добавление зависимости в модуль

```csharp
using Cheetah.Tenants.Client;

[DependsOn(typeof(CrmTenantsClientModule))]
public partial class MyModule : CrmModule
{
    // ...
}
```

### 2. Использование в сервисах

```csharp
using Cheetah.Tenants.Client.Interfaces;

[Export(LifetimeType.Scoped)]
public class MyService
{
    private readonly ITenantClientService _tenantClient;

    public MyService(ITenantClientService tenantClient)
    {
        _tenantClient = tenantClient;
    }

    public async Task DoSomethingAsync(Guid tenantId, CancellationToken ct)
    {
        // Проверить, активен ли тенант
        if (!await _tenantClient.IsActiveAsync(tenantId, ct))
        {
            throw new Exception("Tenant is not active");
        }

        // Получить информацию о тенанте
        var tenant = await _tenantClient.GetByIdAsync(tenantId, ct);
        if (tenant == null)
        {
            throw new Exception("Tenant not found");
        }

        // Получить connection string для тенанта
        var connectionString = await _tenantClient.GetConnectionStringAsync(
            tenantId,
            "Default",
            ct
        );

        // Работать с данными...
    }
}
```

### 3. Использование в Command Handlers

```csharp
[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateUserCommand, Guid>))]
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly ITenantClientService _tenantClient;
    private readonly MyDbContext _dbContext;

    public async ValueTask<Guid> HandleAsync(CreateUserCommand command, CancellationToken ct)
    {
        // Проверяем, что тенант существует
        if (!await _tenantClient.ExistsAsync(command.TenantId, ct))
        {
            throw new InvalidOperationException($"Tenant {command.TenantId} not found");
        }

        // Создаем пользователя
        var user = User.Create(command.Email, command.TenantId);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(ct);

        return user.Id;
    }
}
```

## API

### ITenantClientService

#### GetByIdAsync
```csharp
ValueTask<TenantViewModel?> GetByIdAsync(Guid tenantId, CancellationToken ct = default)
```
Получает тенант по ID.

#### GetBySubdomainAsync
```csharp
ValueTask<TenantViewModel?> GetBySubdomainAsync(string subdomain, CancellationToken ct = default)
```
Получает тенант по поддомену.

#### GetAllActiveAsync
```csharp
ValueTask<List<TenantViewModel>> GetAllActiveAsync(CancellationToken ct = default)
```
Получает список всех активных тенантов.

#### GetConnectionStringAsync
```csharp
ValueTask<string?> GetConnectionStringAsync(Guid tenantId, string name = "Default", CancellationToken ct = default)
```
Получает connection string для указанного тенанта.

#### IsActiveAsync
```csharp
ValueTask<bool> IsActiveAsync(Guid tenantId, CancellationToken ct = default)
```
Проверяет, активен ли тенант.

#### ExistsAsync
```csharp
ValueTask<bool> ExistsAsync(Guid tenantId, CancellationToken ct = default)
```
Проверяет, существует ли тенант.

## Преимущества

1. **Абстракция** - другие модули не зависят от реализации Tenants.Application
2. **Легкий переход на микросервисы** - достаточно заменить реализацию на HTTP-клиент
3. **Единая точка доступа** - все операции с тенантами через один интерфейс
4. **Типобезопасность** - используются ViewModels из Shared проекта

## Зависимости

- `Cheetah.Core.CQRS` - для IDispatcher
- `Cheetah.Tenants.Shared` - для ViewModels
- `Cheetah.Tenants.Application` - для Queries и ITenantStore (временно, для текущей реализации)

## Будущие планы

При переходе на микросервисы:
1. Создать `TenantHttpClientService : ITenantClientService`
2. Заменить регистрацию в `CrmTenantsClientModule`
3. Удалить зависимость от `Cheetah.Tenants.Application`
4. Все модули, использующие `ITenantClientService`, продолжат работать без изменений!
