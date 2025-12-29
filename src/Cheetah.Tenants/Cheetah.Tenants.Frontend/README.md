# Cheetah.Tenants.Frontend

## Описание

Blazor WASM модуль для управления тенантами. Предоставляет полный CRUD интерфейс для работы с тенантами через REST API.

## Структура

```
Cheetah.Tenants.Frontend/
├── Pages/
│   ├── TenantList.razor          # Список всех тенантов
│   ├── TenantCreate.razor        # Создание нового тенанта
│   ├── TenantDetails.razor       # Детали тенанта
│   └── TenantEdit.razor          # Редактирование (placeholder)
├── Components/
│   └── TenantCard.razor          # Карточка тенанта
├── _Imports.razor                # Глобальные usings
└── CrmTenantsFrontendModule.cs   # Модуль регистрации
```

## Роуты

| Роут | Страница | Описание |
|------|----------|----------|
| `/tenants` | TenantList | Список всех тенантов с возможностью активации/деактивации |
| `/tenants/create` | TenantCreate | Форма создания нового тенанта |
| `/tenants/{id}` | TenantDetails | Детальная информация о тенанте |
| `/tenants/{id}/edit` | TenantEdit | Редактирование тенанта (TODO: требует Update API) |

## Страницы

### 1. TenantList (`/tenants`)

**Функционал:**
- Отображение списка всех тенантов в таблице
- Фильтрация по статусу (Active/Inactive)
- Активация/деактивация тенанта кнопками
- Навигация к деталям, редактированию
- Создание нового тенанта

**Состояния:**
- Loading - загрузка данных
- Error - отображение ошибок
- Empty - нет тенантов
- Data - отображение таблицы

**Скриншот структуры:**
```
┌──────────────────────────────────────────────┐
│ Tenants                    [Create New]      │
├──────────────────────────────────────────────┤
│ Name     | Subdomain | Status  | Actions    │
├──────────────────────────────────────────────┤
│ Acme     | acme      | Active  | Details... │
│ Beta Inc | beta      | Inactive| Details... │
└──────────────────────────────────────────────┘
```

### 2. TenantCreate (`/tenants/create`)

**Функционал:**
- Форма с валидацией для создания тенанта
- Поля: Name (обязательное), Subdomain (опциональное)
- Submit создает тенант и переходит на страницу деталей
- Cancel возвращает к списку

**Валидация:**
- Name - обязательное поле
- Subdomain - валидация формата (если заполнено)

### 3. TenantDetails (`/tenants/{id}`)

**Функционал:**
- Отображение всей информации о тенанте
- Показ connection strings
- Кнопки Activate/Deactivate
- Навигация к редактированию
- Возврат к списку

**Разделы:**
- General Information (ID, Name, Subdomain, Status, Created/Updated)
- Connection Strings (таблица с именами и default флагом)
- Actions (Activate/Deactivate)

### 4. TenantEdit (`/tenants/{id}/edit`)

**Статус:** ⚠️ Placeholder - требует реализации Update API

**TODO для полной реализации:**
1. Backend:
   - Создать `UpdateTenantCommand` в Application
   - Создать `UpdateTenantCommandHandler`
   - Добавить `PUT /api/tenants/{id}` endpoint в API
   - Создать `UpdateTenantRequest` в Shared

2. Frontend:
   - Добавить `UpdateAsync` метод в `ITenantApiClient`
   - Реализовать submit form в `TenantEdit.razor`
   - Убрать disabled атрибуты с полей формы

**Текущее поведение:**
- Загружает данные тенанта
- Показывает форму в disabled состоянии
- Показывает информационное сообщение о необходимости реализации Update

## Компоненты

### TenantCard

Переиспользуемый компонент для отображения тенанта в виде карточки.

**Параметры:**
- `Tenant` (required) - данные тенанта
- `ShowActions` (optional, default: true) - показывать ли кнопки действий
- `OnViewDetails` (optional) - callback при клике "View"
- `OnEdit` (optional) - callback при клике "Edit"

**Пример использования:**

```razor
<TenantCard Tenant="@tenant"
            OnViewDetails="@HandleViewDetails"
            OnEdit="@HandleEdit" />

@code {
    private async Task HandleViewDetails(Guid tenantId)
    {
        // Custom logic
    }
}
```

## Использование

### 1. Добавить модуль в зависимости

```csharp
[DependsOn(typeof(CrmTenantsFrontendModule))]
public partial class MyBlazorApp : CrmModule
{
    // ...
}
```

### 2. Добавить навигацию в меню

```razor
<!-- В главном меню приложения -->
<NavLink href="/tenants">
    <i class="bi bi-building"></i> Tenants
</NavLink>
```

### 3. Настроить HttpClient в Program.cs

```csharp
// Program.cs в Blazor WASM приложении
builder.Services.AddHttpClient("BackendAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BackendUrl"]
                                  ?? builder.HostEnvironment.BaseAddress);
});
```

## Зависимости

- `Cheetah.Tenants.Shared` - ViewModels и Requests
- `Cheetah.Tenants.ApiClient` - HTTP клиент для API
- `Cheetah.Frontend.CQRS` - CQRS паттерн для frontend
- `Cheetah.Frontend.Events` - Event bus для frontend
- `Microsoft.AspNetCore.Components.Web` - Blazor компоненты

## Стили и иконки

Проект использует:
- **Bootstrap 5** - для стилизации
- **Bootstrap Icons** - для иконок (префикс `bi bi-*`)

Основные классы:
- `.btn`, `.btn-primary`, `.btn-secondary` - кнопки
- `.table`, `.table-striped` - таблицы
- `.card`, `.card-body` - карточки
- `.badge`, `.bg-success`, `.bg-secondary` - бейджи статуса
- `.alert`, `.alert-danger`, `.alert-info` - уведомления
- `.spinner-border` - индикаторы загрузки

## Примечания

### Обработка ошибок

Все страницы имеют единый подход к обработке ошибок:

```razor
@if (errorMessage != null)
{
    <div class="alert alert-danger" role="alert">
        <i class="bi bi-exclamation-triangle"></i> @errorMessage
    </div>
}
```

### Индикаторы загрузки

Используется Bootstrap spinner:

```razor
@if (isLoading)
{
    <div class="spinner-border" role="status">
        <span class="visually-hidden">Loading...</span>
    </div>
}
```

### Навигация

Все навигационные действия через `NavigationManager`:

```csharp
@inject NavigationManager Navigation

Navigation.NavigateTo("/tenants");
Navigation.NavigateTo($"/tenants/{id}");
```

## Будущие улучшения

- [ ] Реализовать полный Update функционал
- [ ] Добавить пагинацию для списка тенантов
- [ ] Добавить поиск и фильтрацию
- [ ] Добавить сортировку по колонкам
- [ ] Реализовать bulk операции (массовая активация/деактивация)
- [ ] Добавить подтверждение перед деактивацией
- [ ] Добавить управление connection strings через UI
- [ ] Реализовать drag & drop сортировку
