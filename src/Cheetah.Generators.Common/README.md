# Cheetah.Generators.Common

Incremental source generator: для типов со свойствами/параметрами, помеченными `[SyncHash]` (из `Cheetah.Core`), генерирует методы вычисления хэша по выбранному набору полей. Используется для детекции изменений/синхронизации (sync hash) сущностей и DTO.

Подключается как анализатор:
```xml
<ProjectReference Include="..\..\..\Cheetah.Generators.Common\Cheetah.Generators.Common.csproj"
                  ReferenceOutputAssembly="false" OutputItemType="Analyzer" />
```

## Что генерирует

Сканирует всю сборку (включая вложенные namespace) и для каждого типа, где есть хотя бы одно `[SyncHash]`-поле, эмитит extension-метод расчёта хэша по этим полям в заданном порядке (`Order`).

```csharp
public record CustomerSnapshot(
    [property: SyncHash(0)] string Name,
    [property: SyncHash(1)] string Email,
    Guid Id); // не участвует в хэше
```

Поддерживаются оба варианта разметки:
- свойства класса/record;
- параметры первичного конструктора record.

`Order` задаёт детерминированный порядок полей в хэше — два экземпляра с одинаковыми значениями `[SyncHash]`-полей дают одинаковый хэш независимо от объявления.

## Когда использовать

- Понять, изменился ли объект, без сравнения каждого поля.
- Дедупликация/идемпотентность по содержимому.
- Сравнение «снимков» при синхронизации между системами.
