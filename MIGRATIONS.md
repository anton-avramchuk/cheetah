# Database Migrations

## Автоматическая миграция БД

Cheetah CRM поддерживает автоматическое применение миграций при старте приложения.

### Как это работает

1. **При старте приложения** (`CrmEntityFrameworkModule.OnApplicationInitialization`):
   - **Проверяется существование БД** (`Database.CanConnectAsync()`)
   - **Если БД не существует** - создается и применяются все миграции
   - **Если БД существует** - применяются только pending миграции
   - Миграции применяются в порядке зависимостей модулей

2. **Создание БД**:
   - ✅ **БД создается автоматически**, если ее нет
   - ✅ Применяются все миграции сразу после создания
   - ✅ Не нужно создавать БД вручную

3. **Логирование**:
   - Выводится информация о создании БД (если создается)
   - Выводится информация о количестве pending миграций
   - Логируется успешное применение или ошибки

4. **Безопасность**:
   - Миграции применяются последовательно
   - При ошибке - выбрасывается исключение и приложение не стартует

### Создание миграций

#### Для нового модуля

1. **Создать DbContext** с атрибутом ConnectionStringName:
```csharp
[ConnectionStringName("MyModule")]
public class MyDbContext : CrmDbContext<MyDbContext>
{
    public DbSet<MyEntity> MyEntities => Set<MyEntity>();

    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
}
```

2. **Создать IDesignTimeDbContextFactory** для migrations:
```csharp
public class MyDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
{
    public MyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=my_database;Username=user;Password=pass");
        return new MyDbContext(optionsBuilder.Options);
    }
}
```

3. **Зарегистрировать DbContext и Migrator** в модуле:
```csharp
public partial class MyDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        // Register DbContext
        context.Services.AddApplicationDbContext<MyDbContext>();

        // Configure database provider
        context.Services.Configure<CrmDbContextOptions>(options =>
        {
            options.UseNpgsql<MyDbContext>();
        });

        // Register migrator for automatic migrations
        context.Services.AddDatabaseMigrator<MyDbContext>();
    }
}
```

4. **Создать миграцию**:
```bash
cd src/MyModule/MyModule.DataAccess
dotnet ef migrations add InitialCreate --context MyDbContext
```

### Команды для работы с миграциями

#### Создать миграцию
```bash
cd src/Cheetah.Tenants/Cheetah.Tenants.DataAccess
dotnet ef migrations add MigrationName --context TenantsDbContext
```

#### Просмотреть список миграций
```bash
dotnet ef migrations list --context TenantsDbContext
```

#### Удалить последнюю миграцию (если еще не применена)
```bash
dotnet ef migrations remove --context TenantsDbContext
```

#### Создать SQL скрипт миграции
```bash
dotnet ef migrations script --context TenantsDbContext --output migration.sql
```

#### Применить миграции вручную (если нужно)
```bash
dotnet ef database update --context TenantsDbContext
```

### Структура миграций

Миграции находятся в папке `Migrations/` каждого DataAccess проекта:

```
Cheetah.Tenants.DataAccess/
├── Migrations/
│   ├── 20251229145749_InitialCreate.cs          # Код миграции
│   ├── 20251229145749_InitialCreate.Designer.cs # Метаданные
│   └── TenantsDbContextModelSnapshot.cs         # Снимок текущей модели
├── TenantsDbContext.cs
└── TenantsDbContextFactory.cs                   # Для design-time
```

### Архитектура

**Интерфейсы:**
- `IDatabaseMigrator` - интерфейс для мигратора
- `EfCoreMigrator<TDbContext>` - реализация для EF Core
- `DatabaseMigrationManager` - управление всеми миграторами

**Регистрация:**
```csharp
services.AddDatabaseMigrator<MyDbContext>();
```

**Применение:**
```csharp
var manager = serviceProvider.GetRequiredService<DatabaseMigrationManager>();
await manager.MigrateAllAsync();
```

### Connection Strings

#### appsettings.json (production)
```json
{
  "ConnectionStrings": {
    "Tenants": "Host=localhost;Port=5432;Database=cheetah_tenants;Username=cheetah;Password=CHANGE_ME"
  }
}
```

#### appsettings.Development.json (development)
```json
{
  "ConnectionStrings": {
    "Tenants": "Host=localhost;Port=5432;Database=cheetah_tenants;Username=cheetah;Password=cheetah123"
  }
}
```

#### Docker (compose.yaml)
```yaml
environment:
  ConnectionStrings__Tenants: "Host=postgres;Port=5432;Database=cheetah_tenants;Username=cheetah;Password=cheetah123"
```

### Best Practices

1. **Всегда создавайте миграции после изменения модели данных**
2. **Проверяйте сгенерированный код миграции** перед коммитом
3. **Не изменяйте уже примененные миграции** - создавайте новые
4. **Используйте осмысленные имена** для миграций (например, `AddUserEmailIndex`)
5. **Тестируйте миграции** на development окружении перед production
6. **Храните миграции в git** вместе с кодом

### Troubleshooting

#### Миграция не применяется автоматически

Проверьте:
1. Зарегистрирован ли `DatabaseMigrator` для вашего DbContext:
   ```csharp
   services.AddDatabaseMigrator<MyDbContext>();
   ```
2. Правильно ли настроен connection string
3. Доступна ли база данных

#### Ошибка при создании миграции

```
Unable to create a 'DbContext' of type 'MyDbContext'
```

**Решение:** Создайте `IDesignTimeDbContextFactory` (см. выше)

#### Конфликт миграций

Если два разработчика создали миграции параллельно:
1. Удалите свою миграцию: `dotnet ef migrations remove`
2. Подтяните изменения коллеги
3. Создайте миграцию заново

#### Откатить миграцию

```bash
# Откатить до конкретной миграции
dotnet ef database update PreviousMigrationName --context MyDbContext

# Откатить все миграции
dotnet ef database update 0 --context MyDbContext
```

### Multi-Database Architecture

Каждый модуль использует свою базу данных:

- **Tenants Module** → `cheetah_tenants` database
- **Identity Module** → `cheetah_identity` database
- **Features Module** → `cheetah_features` database
- **Permissions Module** → `cheetah_permissions` database

Это обеспечивает:
- Изоляцию данных
- Независимое масштабирование
- Легкость миграции к микросервисам

## Примеры

### Добавление нового поля

```csharp
// 1. Обновить Entity
public class Tenant : AggregateRoot<Guid>
{
    public string Name { get; private set; }
    public string? Email { get; private set; } // NEW FIELD
}

// 2. Создать миграцию
// cd src/Cheetah.Tenants/Cheetah.Tenants.DataAccess
// dotnet ef migrations add AddTenantEmail --context TenantsDbContext

// 3. Миграция применится автоматически при следующем запуске
```

### Создание индекса

```csharp
// В Configuration
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.HasIndex(t => t.Email); // NEW INDEX
    }
}

// dotnet ef migrations add AddTenantEmailIndex --context TenantsDbContext
```

## Заключение

Автоматическая миграция упрощает развертывание и гарантирует, что база данных всегда соответствует текущей модели приложения.

Для production окружений рекомендуется:
- Проверять миграции перед применением
- Создавать резервные копии перед миграцией
- Использовать Blue-Green deployment при критичных изменениях
