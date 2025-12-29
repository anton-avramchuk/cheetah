# Cheetah.Tenants.ApiClient

## Описание

HTTP-клиент для обращения к Tenants API. Используется **только во Frontend (Blazor WASM)** для взаимодействия с backend через REST API.

## Отличие от Cheetah.Tenants.Client

| Модуль | Использование | Технология | Цель |
|--------|---------------|------------|------|
| **Cheetah.Tenants.Client** | Backend модули (Identity, Permissions, Features) | IDispatcher + ITenantStore (прямой доступ к БД) | Для других backend модулей |
| **Cheetah.Tenants.ApiClient** | Frontend (Blazor WASM) | HttpClient (REST API) | Для UI компонентов |

## Архитектура

```
┌─────────────────────────────┐
│  Blazor WASM Frontend       │
│  (Cheetah.Tenants.Frontend) │
└──────────┬──────────────────┘
           │ uses
           ▼
┌─────────────────────────────┐
│  ITenantApiClient           │  ◄─── Интерфейс
└──────────┬──────────────────┘
           │ implements
           ▼
┌─────────────────────────────┐
│  TenantApiClient            │  ◄─── HTTP клиент
│  (HttpClient)               │
└──────────┬──────────────────┘
           │ HTTP requests
           ▼
┌─────────────────────────────┐
│  Backend API                │
│  /api/tenants/*             │
└─────────────────────────────┘
```

## API Endpoints

### GET /api/tenants
Получить все тенанты.
```csharp
var tenants = await _apiClient.GetAllAsync();
```

### GET /api/tenants/{id}
Получить тенант по ID.
```csharp
var tenant = await _apiClient.GetByIdAsync(tenantId);
```

### POST /api/tenants
Создать новый тенант.
```csharp
var request = new CreateTenantRequest { Name = "Acme Corp", Subdomain = "acme" };
var tenantId = await _apiClient.CreateAsync(request);
```

### POST /api/tenants/{id}/activate
Активировать тенант.
```csharp
await _apiClient.ActivateAsync(tenantId);
```

### POST /api/tenants/{id}/deactivate
Деактивировать тенант.
```csharp
await _apiClient.DeactivateAsync(tenantId);
```

## Использование в Blazor компонентах

### 1. Добавить зависимость в модуль

```csharp
[DependsOn(typeof(CrmTenantsApiClientModule))]
public partial class MyFrontendModule : CrmModule
{
    // ...
}
```

### 2. Inject в Razor компонент

```razor
@page "/my-page"
@inject ITenantApiClient TenantApiClient

<h3>Tenants</h3>

@code {
    private List<TenantViewModel>? tenants;

    protected override async Task OnInitializedAsync()
    {
        tenants = await TenantApiClient.GetAllAsync();
    }
}
```

### 3. Пример с обработкой ошибок

```razor
@code {
    private string? errorMessage;
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            tenants = await TenantApiClient.GetAllAsync();
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to load tenants: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

## Конфигурация HttpClient

HttpClient автоматически настраивается в модуле. Базовый URL устанавливается в host приложении:

```csharp
// В Program.cs Blazor WASM приложения
builder.Services.AddHttpClient<TenantApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});
```

## Обработка ошибок

ApiClient использует стандартную обработку HTTP ошибок:

- **404 Not Found**: методы типа `GetByIdAsync` вернут `null`
- **Другие ошибки**: выбрасывается `HttpRequestException`

Пример:

```csharp
try
{
    var tenant = await TenantApiClient.GetByIdAsync(id);
    if (tenant == null)
    {
        // Тенант не найден
    }
}
catch (HttpRequestException ex)
{
    // Ошибка сети или сервера
    Console.WriteLine($"HTTP Error: {ex.Message}");
}
```

## Зависимости

- `Cheetah.Core` - базовые типы и DI
- `Cheetah.Tenants.Shared` - ViewModels и Request DTOs
- `Microsoft.Extensions.Http` - HttpClient factory

## Примечания

- Используется только в **Blazor WASM Frontend**
- Для backend модулей используйте `Cheetah.Tenants.Client`
- Все запросы асинхронные с поддержкой `CancellationToken`
- ViewModels автоматически десериализуются из JSON
