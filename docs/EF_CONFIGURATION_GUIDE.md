# EF Core Configuration Guide

## Правила конфигурации сущностей

### 1. AggregateRoot сущности (ОБЯЗАТЕЛЬНО игнорировать DomainEvents)

#### ✅ Правильно: Использование базовых классов конфигурации

**Для обычных AggregateRoot:**
```csharp
// Сущность
public class MyEntity : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    // ...
}

// Конфигурация - наследуем от AggregateRootConfiguration
public class MyEntityConfiguration : AggregateRootConfiguration<MyEntity, Guid, MyEntityConfigurationOptions>
{
    protected override MyEntityConfigurationOptions Options { get; } = new();

    // base.Configure(builder) автоматически добавит Ignore(e => e.DomainEvents)
}

public class MyEntityConfigurationOptions : AggregateRootConfigurationOptions<MyEntity, Guid>
{
    public override string Schema => "dbo";
    public override string TableName => "MyEntities";
}
```

**Для Tenant-based сущностей:**
```csharp
// Сущность
public class Tenant : TenantEntity<TenantCreatedEvent, TenantUpdatedEvent, TenantDeactivatedEvent, TenantActivatedEvent>
{
    // ...
}

// Конфигурация - наследуем от TenantEntityConfiguration
public class TenantConfiguration : TenantEntityConfiguration<Tenant, TenantCreatedEvent, TenantUpdatedEvent, TenantDeactivatedEvent, TenantActivatedEvent>
{
    protected override TenantEntityConfigurationOptions Options { get; } = new()
    {
        TableName = "Tenants",
        NameMaxLength = 256
    };

    // base.Configure(builder) автоматически добавит Ignore(t => t.DomainEvents)
}
```

#### ✅ Правильно: Прямая конфигурация с явным Ignore

```csharp
// Сущность
public class Feature : AggregateRoot<string>
{
    public string Name { get; private set; } = null!;
    // ...
}

// Конфигурация - IEntityTypeConfiguration с явным Ignore
public class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder.ToTable("Features");
        builder.HasKey(f => f.Id);

        // КРИТИЧЕСКИ ВАЖНО!
        builder.Ignore(f => f.DomainEvents);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(256);
    }
}
```

#### ❌ НЕПРАВИЛЬНО: Забыли Ignore

```csharp
// ❌ БАГ: EF попытается сохранить коллекцию DomainEvents в БД!
public class MyEntityConfiguration : IEntityTypeConfiguration<MyEntity>
{
    public void Configure(EntityTypeBuilder<MyEntity> builder)
    {
        builder.ToTable("MyEntities");
        builder.HasKey(e => e.Id);
        // ❌ Отсутствует builder.Ignore(e => e.DomainEvents);
    }
}
```

### 2. Entity сущности (Entity<TId>)

Entity сущности НЕ имеют DomainEvents, поэтому Ignore НЕ требуется:

```csharp
// Сущность
public class TenantConnectionString : Entity<Guid>
{
    public Guid TenantId { get; private set; }
    public string ConnectionString { get; private set; } = null!;
    // Нет свойства DomainEvents
}

// Конфигурация - можно использовать EntityConfiguration или прямо IEntityTypeConfiguration
public class TenantConnectionStringConfiguration : IEntityTypeConfiguration<TenantConnectionString>
{
    public void Configure(EntityTypeBuilder<TenantConnectionString> builder)
    {
        builder.ToTable("TenantConnectionStrings");
        builder.HasKey(cs => cs.Id);

        // ✅ Ignore НЕ нужен - у Entity нет DomainEvents

        builder.Property(cs => cs.ConnectionString)
            .IsRequired();
    }
}
```

### 3. Базовые конфигурации в Cheetah.Core

#### AggregateRootConfiguration (автоматически игнорирует DomainEvents)

**Расположение:** `src/Cheetah.Core.EntityFramework/Configuration/AggregateRootConfiguration.cs`

```csharp
public abstract class AggregateRootConfiguration<TEntity, TKey, TConfiguration> :
    EntityConfiguration<TEntity, TKey, TConfiguration>
    where TEntity : AggregateRoot<TKey>
    where TConfiguration : AggregateRootConfigurationOptions<TEntity, TKey>
{
    public override void Configure(EntityTypeBuilder<TEntity> builder)
    {
        base.Configure(builder);

        // ✅ Автоматически игнорирует DomainEvents
        builder.Ignore(x => x.DomainEvents);
    }
}
```

**Преимущества:**
- Автоматический Ignore DomainEvents
- Автоматическая конфигурация Id, таблицы, схемы
- Централизованное управление базовыми правилами

#### TenantEntityConfiguration (автоматически игнорирует DomainEvents)

**Расположение:** `src/Cheetah.Core.EntityFramework.Tenants/Configurations/TenantEntityConfiguration.cs`

```csharp
public abstract class TenantEntityConfiguration<TTenant, ...> : IEntityTypeConfiguration<TTenant>
    where TTenant : TenantEntity<...>
{
    public virtual void Configure(EntityTypeBuilder<TTenant> builder)
    {
        builder.ToTable(Options.TableName);
        builder.HasKey(t => t.Id);

        // ✅ Автоматически игнорирует DomainEvents
        builder.Ignore(t => t.DomainEvents);

        // ... остальная конфигурация
    }
}
```

**Преимущества:**
- Автоматический Ignore DomainEvents
- Предконфигурированные индексы для Name, IsActive, CreatedAt
- Единообразная структура для всех Tenant сущностей

### 4. Текущий статус проекта

#### ✅ Все модули правильно конфигурированы:

**Tenants модуль:**
- `TenantConfiguration` → наследует `TenantEntityConfiguration` → ✅ есть Ignore

**Identity модуль:**
- `UserConfiguration` → прямой `IEntityTypeConfiguration` → ✅ есть прямой Ignore на строке 52
- `RoleConfiguration` → прямой `IEntityTypeConfiguration` → ✅ есть прямой Ignore на строке 36

**Features модуль:**
- `FeatureConfiguration` → прямой `IEntityTypeConfiguration` → ✅ есть прямой Ignore на строке 44

**IdentityCore (инфраструктура):**
- `IdentityUserConfiguration` → наследует `AggregateRootConfiguration` → ✅ автоматический Ignore
- `IdentityRoleConfiguration` → наследует `AggregateRootConfiguration` → ✅ автоматический Ignore

**Entity сущности (не требуют Ignore):**
- `TenantConnectionString` → наследует `Entity<Guid>` → ✅ корректно
- `TenantFeature` → наследует `Entity<Guid>` → ✅ корректно
- `UserRole` → наследует `IdentityUserRole` → `Entity` → ✅ корректно
- `UserClaim` → наследует `IdentityUserClaim` → `IdentityClaim` → `Entity<Guid>` → ✅ корректно
- `RoleClaim` → наследует `IdentityRoleClaim` → `IdentityClaim` → `Entity<Guid>` → ✅ корректно

### 5. Чеклист для новых модулей

При создании нового модуля:

- [ ] Определите, какие сущности являются AggregateRoot (имеют бизнес-логику, генерируют события)
- [ ] Для каждого AggregateRoot создайте конфигурацию:
  - [ ] **Вариант 1 (рекомендуется):** Наследуйте от `AggregateRootConfiguration<TEntity, TKey, TOptions>`
  - [ ] **Вариант 2:** Используйте `IEntityTypeConfiguration<T>` с ОБЯЗАТЕЛЬНЫМ `builder.Ignore(e => e.DomainEvents)`
- [ ] Для Entity сущностей (без DomainEvents) используйте `EntityConfiguration` или `IEntityTypeConfiguration`
- [ ] Проверьте, что в конфигурации есть:
  - [ ] `builder.HasKey()`
  - [ ] `builder.ToTable()`
  - [ ] Валидация длин строк (`HasMaxLength()`)
  - [ ] Обязательные поля (`IsRequired()`)
  - [ ] Индексы для часто запрашиваемых полей
  - [ ] `builder.Ignore(e => e.DomainEvents)` для AggregateRoot (если не используете базовые классы)

### 6. Частые ошибки

#### ❌ Ошибка 1: Забыли Ignore для AggregateRoot

```csharp
// ❌ Будет runtime ошибка при SaveChanges!
public class MyAggregateConfiguration : IEntityTypeConfiguration<MyAggregate>
{
    public void Configure(EntityTypeBuilder<MyAggregate> builder)
    {
        builder.ToTable("MyAggregates");
        // ❌ Забыли builder.Ignore(e => e.DomainEvents);
    }
}
```

**Симптомы:**
- `InvalidOperationException` при вызове `SaveChangesAsync()`
- Ошибка миграции: "No suitable constructor found for entity type 'List<IDomainEvent>'"

**Решение:**
```csharp
// ✅ Добавьте Ignore
builder.Ignore(e => e.DomainEvents);
```

#### ❌ Ошибка 2: Использование Ignore для Entity

```csharp
// ⚠️ Не ошибка, но избыточно
public class MyEntityConfiguration : IEntityTypeConfiguration<MyEntity>
{
    public void Configure(EntityTypeBuilder<MyEntity> builder)
    {
        builder.ToTable("MyEntities");

        // ⚠️ MyEntity наследует от Entity<TId>, у него нет DomainEvents
        builder.Ignore(e => e.DomainEvents); // Компилятор ошибка!
    }
}
```

**Решение:**
- Ignore нужен только для AggregateRoot
- Entity не имеет свойства DomainEvents

### 7. Примеры из проекта

#### Пример 1: Tenant (использует TenantEntityConfiguration)

**Сущность:**
```csharp
// src/Cheetah.Tenants/Cheetah.Tenants.Domain/Entities/Tenant.cs
public class Tenant : TenantEntity<TenantCreatedEvent, TenantUpdatedEvent, TenantDeactivatedEvent, TenantActivatedEvent>
{
    public string NormalizedName { get; private set; } = null!;
    public string? Subdomain { get; private set; }
    // ...
}
```

**Конфигурация:**
```csharp
// src/Cheetah.Tenants/Cheetah.Tenants.DataAccess/Configurations/TenantConfiguration.cs
public class TenantConfiguration : TenantEntityConfiguration<Tenant, TenantCreatedEvent, TenantUpdatedEvent, TenantDeactivatedEvent, TenantActivatedEvent>
{
    protected override TenantEntityConfigurationOptions Options { get; } = new()
    {
        TableName = "Tenants",
        NameMaxLength = TenantConstants.MaxNameLength,
        CreateNameUniqueIndex = true
    };

    protected override void ConfigureAdditionalProperties(EntityTypeBuilder<Tenant> builder)
    {
        builder.Property(t => t.NormalizedName)
            .IsRequired()
            .HasMaxLength(TenantConstants.MaxNormalizedNameLength);

        builder.HasIndex(t => t.NormalizedName).IsUnique();
    }

    // ✅ Ignore автоматически добавляется в TenantEntityConfiguration.Configure()
}
```

#### Пример 2: User (использует прямой IEntityTypeConfiguration)

**Сущность:**
```csharp
// src/Cheetah.Identity/Cheetah.Identity.Domain/Entities/User.cs
public class User : IdentityUser<Role>
{
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    // ...
}
```

**Конфигурация:**
```csharp
// src/Cheetah.Identity/Cheetah.Identity.DataAccess/Configurations/UserConfiguration.cs
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        // ✅ КРИТИЧЕСКИ ВАЖНО: Явный Ignore
        builder.Ignore(u => u.DomainEvents);

        builder.HasMany(u => u.Roles)
            .WithOne()
            .HasForeignKey(ur => ur.UserId);
    }
}
```

#### Пример 3: Feature (использует прямой IEntityTypeConfiguration)

**Сущность:**
```csharp
// src/Cheetah.Features/Cheetah.Features.Domain/Entities/Feature.cs
public class Feature : AggregateRoot<string>
{
    public string Name { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    // ...
}
```

**Конфигурация:**
```csharp
// src/Cheetah.Features/Cheetah.Features.DataAccess/Configurations/FeatureConfiguration.cs
public class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder.ToTable("Features");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(FeatureConstants.FeatureIdMaxLength);

        // ✅ КРИТИЧЕСКИ ВАЖНО: Явный Ignore
        builder.Ignore(f => f.DomainEvents);

        builder.HasIndex(f => f.Group);
    }
}
```

## Заключение

**Главное правило:** Каждая сущность, наследующая от `AggregateRoot<TKey>`, **ОБЯЗАНА** иметь `builder.Ignore(e => e.DomainEvents)` в своей EF конфигурации.

**Рекомендации:**
1. Используйте базовые классы (`AggregateRootConfiguration`, `TenantEntityConfiguration`) - они автоматически добавляют Ignore
2. Если используете прямой `IEntityTypeConfiguration` - обязательно добавьте явный Ignore
3. Для Entity сущностей Ignore не нужен (у них нет DomainEvents)

**Все текущие модули проекта соответствуют этим правилам! ✅**
