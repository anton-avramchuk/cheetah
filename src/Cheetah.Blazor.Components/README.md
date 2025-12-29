# Cheetah.Frontend.Components

## Описание

Библиотека переиспользуемых UI компонентов для Blazor WASM приложений. Все компоненты стилизованы с Bootstrap 5 и поддерживают Bootstrap Icons.

## Установка

Добавьте зависимость в ваш Frontend модуль:

```csharp
[DependsOn(typeof(CrmFrontendComponentsModule))]
public partial class MyFrontendModule : CrmModule
{
    // ...
}
```

Добавьте в `_Imports.razor`:

```razor
@using Cheetah.Frontend.Components.Buttons
@using Cheetah.Frontend.Components.DataGrid
@using Cheetah.Frontend.Components.Forms
@using Cheetah.Frontend.Components.Layout
@using Cheetah.Frontend.Components.Feedback
@using Cheetah.Frontend.Components.Navigation
```

## Компоненты

### 1. CrmButton

Универсальная кнопка с поддержкой различных стилей, размеров, иконок и состояния загрузки.

**Параметры:**
- `Variant` - Primary, Secondary, Success, Danger, Warning, Info, Light, Dark, Link
- `Size` - Small, Normal, Large
- `Type` - button, submit, reset
- `Disabled` - отключена
- `IsLoading` - показать spinner
- `Outline` - стиль outline
- `FullWidth` - на всю ширину
- `Icon` - иконка Bootstrap (без префикса 'bi-')
- `Text` - текст кнопки
- `OnClick` - обработчик клика

**Примеры:**

```razor
@* Простая primary кнопка *@
<CrmButton Text="Save" OnClick="@HandleSave" />

@* Кнопка с иконкой *@
<CrmButton Text="Delete"
           Icon="trash"
           Variant="CrmButton.ButtonVariant.Danger"
           OnClick="@HandleDelete" />

@* Кнопка загрузки *@
<CrmButton Text="Loading..."
           IsLoading="@isLoading"
           Disabled="@isLoading" />

@* Outline кнопка *@
<CrmButton Text="Cancel"
           Variant="CrmButton.ButtonVariant.Secondary"
           Outline="true" />

@* Маленькая кнопка *@
<CrmButton Text="Edit"
           Size="CrmButton.ButtonSize.Small"
           Icon="pencil" />

@* Submit кнопка в форме *@
<CrmButton Text="Submit"
           Type="submit"
           Variant="CrmButton.ButtonVariant.Success" />
```

### 2. CrmLoadingSpinner

Индикатор загрузки с опциональным сообщением.

**Параметры:**
- `Message` - текст сообщения
- `Size` - Small, Normal, Large
- `Variant` - Border, Grow
- `Color` - Primary, Secondary, Success, Danger, Warning, Info, Light, Dark
- `Center` - центрировать по центру

**Примеры:**

```razor
@* Простой spinner *@
<CrmLoadingSpinner />

@* С сообщением *@
<CrmLoadingSpinner Message="Loading data..." />

@* Большой зеленый spinner *@
<CrmLoadingSpinner Size="CrmLoadingSpinner.SpinnerSize.Large"
                   Color="CrmLoadingSpinner.SpinnerColor.Success" />

@* Grow вариант *@
<CrmLoadingSpinner Variant="CrmLoadingSpinner.SpinnerVariant.Grow" />
```

### 3. CrmAlert

Уведомления и сообщения с возможностью закрытия.

**Параметры:**
- `Variant` - Primary, Secondary, Success, Danger, Warning, Info, Light, Dark
- `Message` - текст сообщения
- `Icon` - иконка Bootstrap
- `Dismissible` - можно закрыть
- `IsVisible` - видимость
- `OnDismissed` - callback при закрытии

**Примеры:**

```razor
@* Успешное сообщение *@
<CrmAlert Variant="CrmAlert.AlertVariant.Success"
          Message="Operation completed successfully!"
          Icon="check-circle" />

@* Ошибка с возможностью закрытия *@
<CrmAlert Variant="CrmAlert.AlertVariant.Danger"
          Message="@errorMessage"
          Icon="exclamation-triangle"
          Dismissible="true" />

@* С custom content *@
<CrmAlert Variant="CrmAlert.AlertVariant.Info">
    <strong>Note:</strong> This is important information.
</CrmAlert>
```

### 4. CrmBadge

Бейджи для отображения статусов и меток.

**Параметры:**
- `Variant` - Primary, Secondary, Success, Danger, Warning, Info, Light, Dark
- `Text` - текст бейджа
- `Icon` - иконка Bootstrap
- `Pill` - скругленный стиль

**Примеры:**

```razor
@* Статус активен *@
<CrmBadge Text="Active"
          Variant="CrmBadge.BadgeVariant.Success" />

@* С иконкой *@
<CrmBadge Text="5"
          Icon="envelope"
          Variant="CrmBadge.BadgeVariant.Primary" />

@* Pill стиль *@
<CrmBadge Text="New"
          Variant="CrmBadge.BadgeVariant.Info"
          Pill="true" />
```

### 5. CrmTextInput

Текстовое поле ввода с валидацией и подсказками.

**Параметры:**
- `@bind-Value` - двухстороннее связывание
- `Label` - метка поля
- `Placeholder` - подсказка
- `HelpText` - текст помощи
- `InputType` - text, password, email, number, etc.
- `Disabled` - отключено
- `ReadOnly` - только чтение
- `Required` - обязательное (добавляет звездочку)
- `ValidationFor` - expression для валидации

**Примеры:**

```razor
<EditForm Model="@model" OnValidSubmit="@HandleSubmit">
    <DataAnnotationsValidator />

    @* Простое текстовое поле *@
    <CrmTextInput @bind-Value="model.Name"
                  Label="Name"
                  Placeholder="Enter name"
                  Required="true"
                  ValidationFor="@(() => model.Name)" />

    @* Email поле *@
    <CrmTextInput @bind-Value="model.Email"
                  Label="Email"
                  InputType="email"
                  HelpText="We'll never share your email."
                  ValidationFor="@(() => model.Email)" />

    @* Password *@
    <CrmTextInput @bind-Value="model.Password"
                  Label="Password"
                  InputType="password"
                  ValidationFor="@(() => model.Password)" />

    @* Disabled field *@
    <CrmTextInput @bind-Value="model.Id"
                  Label="ID"
                  Disabled="true" />
</EditForm>
```

### 6. CrmCard

Карточка для группировки контента.

**Параметры:**
- `Title` - заголовок
- `Subtitle` - подзаголовок
- `Header` - custom header content
- `Footer` - custom footer content
- `ImageUrl` - URL изображения
- `ImageAlt` - alt текст для изображения
- `Variant` - вариант рамки (Primary, Secondary, etc.)

**Примеры:**

```razor
@* Простая карточка *@
<CrmCard Title="User Profile">
    <p>User information goes here.</p>
</CrmCard>

@* С header и footer *@
<CrmCard>
    <Header>
        <div class="d-flex justify-content-between">
            <h5>Statistics</h5>
            <CrmButton Text="Refresh" Size="CrmButton.ButtonSize.Small" />
        </div>
    </Header>
    <ChildContent>
        <p>Chart and statistics content.</p>
    </ChildContent>
    <Footer>
        <small class="text-muted">Last updated 5 mins ago</small>
    </Footer>
</CrmCard>

@* С изображением *@
<CrmCard Title="Product Name"
         Subtitle="$99.99"
         ImageUrl="/images/product.jpg">
    <p>Product description.</p>
</CrmCard>
```

### 7. CrmDataGrid

Универсальная таблица данных с сортировкой и пагинацией.

**Параметры:**
- `Items` - коллекция данных
- `Columns` - template для заголовков
- `RowTemplate` - template для строк
- `IsLoading` - показать spinner
- `ErrorMessage` - сообщение об ошибке
- `EmptyMessage` - сообщение когда нет данных
- `Striped` - полосатый стиль
- `Hover` - hover эффект
- `Bordered` - с рамками
- `Small` - компактный размер
- `ShowPagination` - показать пагинацию
- `CurrentPage` - текущая страница
- `TotalPages` - общее количество страниц
- `OnPageChanged` - callback смены страницы

**Примеры:**

```razor
<CrmDataGrid Items="@tenants"
             IsLoading="@isLoading"
             ErrorMessage="@errorMessage">
    <Columns>
        <th>Name</th>
        <th>Status</th>
        <th>Created</th>
        <th>Actions</th>
    </Columns>
    <RowTemplate Context="tenant">
        <td>@tenant.Name</td>
        <td>
            <CrmBadge Text="@(tenant.IsActive ? "Active" : "Inactive")"
                      Variant="@(tenant.IsActive ? CrmBadge.BadgeVariant.Success : CrmBadge.BadgeVariant.Secondary)" />
        </td>
        <td>@tenant.CreatedAt?.ToString("yyyy-MM-dd")</td>
        <td>
            <CrmButton Text="Details"
                       Size="CrmButton.ButtonSize.Small"
                       OnClick="@(() => ViewDetails(tenant.Id))" />
        </td>
    </RowTemplate>
</CrmDataGrid>

@* С пагинацией *@
<CrmDataGrid Items="@pagedItems"
             IsLoading="@isLoading"
             ShowPagination="true"
             CurrentPage="@currentPage"
             TotalPages="@totalPages"
             OnPageChanged="@HandlePageChanged">
    @* ... columns and rows ... *@
</CrmDataGrid>

@code {
    private async Task HandlePageChanged(int newPage)
    {
        currentPage = newPage;
        await LoadDataAsync();
    }
}
```

### 8. CrmPagination

Компонент пагинации (используется автоматически в CrmDataGrid).

**Параметры:**
- `CurrentPage` - текущая страница (1-indexed)
- `TotalPages` - общее количество страниц
- `MaxVisiblePages` - максимум видимых кнопок страниц
- `Alignment` - Start, Center, End
- `OnPageChanged` - callback смены страницы

**Примеры:**

```razor
<CrmPagination CurrentPage="@currentPage"
               TotalPages="@totalPages"
               OnPageChanged="@HandlePageChanged" />

@* Центрированная пагинация *@
<CrmPagination CurrentPage="@currentPage"
               TotalPages="@totalPages"
               Alignment="CrmPagination.PaginationAlignment.Center"
               OnPageChanged="@HandlePageChanged" />
```

## Полный пример использования

```razor
@page "/users"
@using Cheetah.Frontend.Components.Buttons
@using Cheetah.Frontend.Components.DataGrid
@using Cheetah.Frontend.Components.Feedback

<div class="container">
    <div class="d-flex justify-content-between align-items-center mb-3">
        <h3>Users</h3>
        <CrmButton Text="Create New"
                   Icon="plus-circle"
                   Variant="CrmButton.ButtonVariant.Primary"
                   OnClick="@NavigateToCreate" />
    </div>

    @if (successMessage != null)
    {
        <CrmAlert Variant="CrmAlert.AlertVariant.Success"
                  Message="@successMessage"
                  Icon="check-circle"
                  Dismissible="true"
                  @bind-IsVisible="@showSuccess" />
    }

    <CrmDataGrid Items="@users"
                 IsLoading="@isLoading"
                 ErrorMessage="@errorMessage"
                 EmptyMessage="No users found. Click 'Create New' to add one."
                 ShowPagination="true"
                 CurrentPage="@currentPage"
                 TotalPages="@totalPages"
                 OnPageChanged="@LoadUsersAsync">
        <Columns>
            <th>Name</th>
            <th>Email</th>
            <th>Status</th>
            <th>Role</th>
            <th>Actions</th>
        </Columns>
        <RowTemplate Context="user">
            <td>@user.Name</td>
            <td>@user.Email</td>
            <td>
                <CrmBadge Text="@(user.IsActive ? "Active" : "Inactive")"
                          Variant="@(user.IsActive ? CrmBadge.BadgeVariant.Success : CrmBadge.BadgeVariant.Secondary)" />
            </td>
            <td>
                <CrmBadge Text="@user.Role"
                          Variant="CrmBadge.BadgeVariant.Info"
                          Pill="true" />
            </td>
            <td>
                <CrmButton Text="Edit"
                           Icon="pencil"
                           Size="CrmButton.ButtonSize.Small"
                           Variant="CrmButton.ButtonVariant.Warning"
                           OnClick="@(() => EditUser(user.Id))" />
                <CrmButton Text="Delete"
                           Icon="trash"
                           Size="CrmButton.ButtonSize.Small"
                           Variant="CrmButton.ButtonVariant.Danger"
                           Outline="true"
                           OnClick="@(() => DeleteUser(user.Id))" />
            </td>
        </RowTemplate>
    </CrmDataGrid>
</div>

@code {
    private List<UserViewModel> users = new();
    private bool isLoading = true;
    private string? errorMessage;
    private string? successMessage;
    private bool showSuccess = true;
    private int currentPage = 1;
    private int totalPages = 1;

    protected override async Task OnInitializedAsync()
    {
        await LoadUsersAsync(1);
    }

    private async Task LoadUsersAsync(int page)
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            // Load users from API
            users = await ApiClient.GetUsersAsync(page);
            currentPage = page;
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to load users: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

## Стили и иконки

Все компоненты используют:
- **Bootstrap 5** - для стилей и layout
- **Bootstrap Icons** - для иконок

Убедитесь, что в вашем приложении подключены:

```html
<!-- Bootstrap CSS -->
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">

<!-- Bootstrap Icons -->
<link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/bootstrap-icons.css" rel="stylesheet">
```

## Доступные компоненты

| Компонент | Категория | Описание |
|-----------|-----------|----------|
| `CrmButton` | Buttons | Универсальная кнопка |
| `CrmTextInput` | Forms | Текстовое поле с валидацией |
| `CrmLoadingSpinner` | Feedback | Индикатор загрузки |
| `CrmAlert` | Feedback | Уведомления и сообщения |
| `CrmBadge` | Feedback | Бейджи статусов |
| `CrmCard` | Layout | Карточки контента |
| `CrmDataGrid` | DataGrid | Таблица данных |
| `CrmPagination` | Navigation | Пагинация |

## TODO - Будущие компоненты

- [ ] `CrmModal` - Модальные окна
- [ ] `CrmSelect` - Выпадающий список
- [ ] `CrmCheckbox` - Чекбокс
- [ ] `CrmRadio` - Радио кнопки
- [ ] `CrmDatePicker` - Выбор даты
- [ ] `CrmToast` - Toast уведомления
- [ ] `CrmTabs` - Вкладки
- [ ] `CrmAccordion` - Аккордеон
- [ ] `CrmBreadcrumb` - Хлебные крошки
- [ ] `CrmDropdown` - Dropdown меню

## Примечания

- Все компоненты полностью типизированы и поддерживают IntelliSense
- Компоненты используют Bootstrap 5 классы и могут быть кастомизированы через `CssClass` параметр
- DataGrid поддерживает generic типы для любых моделей данных
- Все компоненты поддерживают nullable reference types

## Миграция с нативных компонентов

Если вы используете нативные HTML элементы, вот как мигрировать на Cheetah компоненты:

### Кнопки
```razor
<!-- Было -->
<button class="btn btn-primary" @onclick="HandleClick">Save</button>

<!-- Стало -->
<CrmButton Text="Save" OnClick="@HandleClick" />
```

### Таблицы
```razor
<!-- Было -->
<table class="table table-striped">
    <thead>
        <tr><th>Name</th></tr>
    </thead>
    <tbody>
        @foreach (var item in items)
        {
            <tr><td>@item.Name</td></tr>
        }
    </tbody>
</table>

<!-- Стало -->
<CrmDataGrid Items="@items">
    <Columns><th>Name</th></Columns>
    <RowTemplate Context="item">
        <td>@item.Name</td>
    </RowTemplate>
</CrmDataGrid>
```

### Формы
```razor
<!-- Было -->
<label for="name">Name</label>
<input id="name" class="form-control" @bind="model.Name" />
<ValidationMessage For="@(() => model.Name)" />

<!-- Стало -->
<CrmTextInput @bind-Value="model.Name"
              Label="Name"
              ValidationFor="@(() => model.Name)" />
```
