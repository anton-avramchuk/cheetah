# Cheetah Custom Mapping Module (Expressions)

Этот модуль предоставляет кастомную реализацию `IObjectMapper` для фреймворка Cheetah, основанную на динамической генерации деревьев выражений (`System.Linq.Expressions`).

Модуль является альтернативой `Cheetah.Mapping.Mapster` и создан для полного контроля над процессами маппинга и строгой валидации.

## 🚀 Особенности

- **Высокая производительность в Runtime:** Маппинг компилируется в делегаты один раз и кэшируется. Скорость работы сопоставима с ручным написанием маппинга.
- **Поддержка IQueryable (Entity Framework Core):** Генерация `Expression` для метода `.ProjectTo<TDest>()`, что позволяет выполнять маппинг прямо на стороне базы данных (SQL), загружая только нужные колонки.
- **Строгая валидация (Strict Validation):** Механизм не позволяет "тихо" проигнорировать отсутствующее свойство. Если вы добавили свойство в DTO, но его нет в сущности (Source), маппер выбросит `InvalidOperationException` при первом обращении.
- **Рекурсивный маппинг:** Поддержка маппинга вложенных объектов с автоматической обработкой null-значений (`source.Nested == null ? null : new NestedDto { ... }`).
- **Поддержка Nullable типов:** Автоматическая конвертация `T` <-> `Nullable<T>`.

## 📦 Подключение к вашему модулю

Чтобы перевести ваш API или Application модуль на этот маппер, достаточно изменить атрибут `[DependsOn]` в классе вашего модуля.

**До:**
```csharp
[DependsOn(typeof(CrmMapsterModule))]
public partial class BiolabApplicationModule : CrmModule { ... }
```

**После:**
```csharp
using Cheetah.Mapping.Expressions;

[DependsOn(typeof(CrmCustomMappingModule))]
public partial class BiolabApplicationModule : CrmModule { ... }
```

## 🛠️ Использование

Интерфейс остается стандартным (`IObjectMapper` из `Cheetah.Mapping.Core`), поэтому вам не нужно менять бизнес-логику.

### 1. Рантайм маппинг (объекты в памяти)

```csharp
public class GetUserHandler
{
    private readonly IObjectMapper _mapper;

    public GetUserHandler(IObjectMapper mapper)
    {
        _mapper = mapper;
    }

    public void Handle(User entity)
    {
        // Выполняет быстрое копирование свойств через скомпилированные делегаты
        var dto = _mapper.Map<User, UserDto>(entity);
    }
}
```

### 2. Маппинг через IQueryable (EF Core)

```csharp
public class UserRepository
{
    private readonly MyDbContext _context;
    private readonly IObjectMapper _mapper;

    public async Task<List<UserDto>> GetUsersAsync()
    {
        return await _context.Users
            .AsNoTracking()
            // Генерирует SQL: SELECT "Id", "Name" FROM "Users"
            .ProjectTo<UserDto>(_mapper) 
            .ToListAsync();
    }
}
```

## 🛡️ Строгая валидация (Пример ошибки)

Если ваша сущность выглядит так:
```csharp
public class User { public Guid Id { get; set; } }
```
А DTO требует дополнительное поле:
```csharp
public class UserDto { 
    public Guid Id { get; set; } 
    public string Name { get; set; } // Этого поля нет в User!
}
```

При попытке выполнить `_mapper.Map<User, UserDto>(user)` приложение **сразу выбросит исключение**:
> `InvalidOperationException: Strict Validation Failed: Unmapped property 'UserDto.Name'! Source type 'User' does not have a corresponding property.`

Это защищает от ситуаций, когда разработчик добавил новое поле в ответ API, но забыл достать его из базы.

## ⚠️ Ограничения MVP (Текущей версии)

На данный момент модуль **не поддерживает**:
- Автоматический маппинг коллекций (`List<T>`, `Array`).
- Маппинг в существующий объект (`Map(source, destination)`).
- Пользовательские конфигурации (Игнорирование полей или `MapFrom` через Fluent API).

*Если вам нужна поддержка атрибутов и полная валидация во время компиляции, используйте параллельный модуль генерации кода: `Cheetah.Mapping.Generators`.*
