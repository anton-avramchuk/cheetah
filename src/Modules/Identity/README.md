# Cheetah Module Template

Шаблон для создания нового модуля Cheetah CRM с полной структурой проектов.

## Установка

```bash
dotnet new install <путь-к-templates/cheetah-module>
```

## Использование

```bash
dotnet new cheetah-module -n <ИмяМодуля> [опции]
```

### Параметры

| Параметр | Описание | По умолчанию | Пример |
|----------|----------|--------------|--------|
| `-n, --name` | Имя модуля (PascalCase) | *обязательный* | `Orders`, `Products` |
| `--prefix` | Префикс namespace | `Crm` | `Cheetah.Admin.Modules` |
| `--entity` | Имя сущности (PascalCase) | `Entity` | `Order`, `Product` |
| `-o, --output` | Путь для генерации | текущая папка | `src/Modules/Orders` |

> **Рекомендация:** Всегда указывайте `--entity` для осмысленного именования сущности.
> Схема БД автоматически генерируется из имени модуля в lowercase (`Orders` → `orders`).

### Примеры

**Простой модуль с префиксом Crm:**
```bash
dotnet new cheetah-module -n Products --entity Product -o src/Modules/Products
```

**Модуль для Admin API:**
```bash
dotnet new cheetah-module -n Orders --prefix Cheetah.Admin.Modules --entity Order -o src/Modules/Admin/Orders
```

## Генерируемая структура

Шаблон создает 11 проектов:

```
{Prefix}.{Identity}/
├── {Prefix}.{Identity}.DomainEvents/     # События домена
├── {Prefix}.{Identity}.Domain/           # Сущности, интерфейсы репозиториев
├── {Prefix}.{Identity}.Contracts/        # DTO, Requests, ViewModels
├── {Prefix}.{Identity}.DataAccess/       # DbContext, конфигурации EF Core, репозитории
├── {Prefix}.{Identity}.Application/      # Commands, Queries, Handlers (CQRS)
├── {Prefix}.{Identity}.ApiClient/        # HTTP клиент для интеграции
├── {Prefix}.{Identity}.Api/              # Minimal API endpoints
├── {Prefix}.{Identity}.Domain.Tests/     # Unit тесты Domain
├── {Prefix}.{Identity}.Application.Tests/# Unit тесты Application
├── {Prefix}.{Identity}.ApiClient.Tests/  # Unit тесты ApiClient
└── {Prefix}.{Identity}.Api.Tests/        # Integration тесты API
```

## После генерации

### 1. Добавить проекты в solution

```bash
dotnet sln Cheetah.slnx add src/Modules/{Identity}/**/*.csproj
```

### 2. Настроить connection string

В `appsettings.json` хост-приложения добавить:

```json
{
  "ConnectionStrings": {
    "{Identity}": "Host=localhost;Database=cheetah;Username=postgres;Password=postgres"
  }
}
```

### 3. Создать миграцию

```bash
dotnet ef migrations add Initial -p src/Modules/{Identity}/{Prefix}.{Identity}.DataAccess -s src/Hosts/Cheetah.Admin.Api
```

### 4. Подключить модуль к хосту

В bootstrapper модуле хоста добавить зависимость:

```csharp
[DependsOn(typeof({Identity}BootstrapperModule))]
public partial class AdminApiBootstrapperModule : CrmModule
```

## Структура сгенерированного кода

### Domain Layer

- **{Entity}.cs** - Агрегат с factory method `Create()` и методом `Update()`
- **I{Entity}Repository.cs** - Интерфейс репозитория

### DataAccess Layer

- **{Identity}DbContext.cs** - DbContext с конфигурацией
- **{Entity}Configuration.cs** - EF Core конфигурация сущности
- **{Entity}Repository.cs** - Реализация репозитория

### Contracts Layer

- **{Entity}ViewModel.cs** - DTO для API ответов
- **Create{Entity}Request.cs** - Запрос на создание
- **Update{Entity}Request.cs** - Запрос на обновление
- **Get{Entity}ByIdRequest.cs** - Запрос по ID
- **GetAll{Entities}Request.cs** - Запрос списка
- **Delete{Entity}Request.cs** - Запрос на удаление

### Application Layer

- **{Entity}Model.cs** - Модель приложения
- **Commands/** - CreateCommand, UpdateCommand, DeleteCommand + Handlers
- **Queries/** - GetByIdQuery, GetAllQuery + Handlers

### Api Layer

- **Endpoints/** - CRUD endpoints (Create, GetAll, GetById, Update, Delete)
- **Mapping/MappingProfile.cs** - Mapster профиль маппинга
- **Constants.cs** - Константы роутов

### ApiClient Layer

- **I{Identity}Service.cs** - Интерфейс HTTP клиента
- **{Identity}Service.cs** - Реализация HTTP клиента

## API Endpoints

| Method | Route | Описание |
|--------|-------|----------|
| POST | /api/{schema} | Создать сущность |
| GET | /api/{schema} | Получить все сущности |
| GET | /api/{schema}/{id} | Получить по ID |
| PUT | /api/{schema}/{id} | Обновить сущность |
| DELETE | /api/{schema}/{id} | Удалить сущность |

## Тестирование

```bash
# Запуск всех тестов модуля
dotnet test src/Modules/{Identity}/

# Только unit тесты
dotnet test src/Modules/{Identity}/{Prefix}.{Identity}.Domain.Tests/
dotnet test src/Modules/{Identity}/{Prefix}.{Identity}.Application.Tests/

# Integration тесты (требуют Docker для PostgreSQL)
dotnet test src/Modules/{Identity}/{Prefix}.{Identity}.Api.Tests/
```

## Зависимости между проектами

```
DomainEvents (no deps)
       ↓
    Domain ← DataAccess
       ↓         ↓
  Contracts      ↓
       ↓         ↓
 Application ←───┘
       ↓
      Api ← ApiClient
```
