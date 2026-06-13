# Cheetah.Mapping.Mapster

Реализация `IObjectMapper` ([Cheetah.Mapping.Core](../Cheetah.Mapping.Core/README.md)) на базе **Mapster**. Дефолтный маппер проекта; подключается через `CrmMapsterModule` (см. правила Api-слоя в корневом CLAUDE.md). Зависит от `Cheetah.Core`, `CrmMappingCoreModule`.

## Состав

| Тип | Назначение |
|-----|------------|
| `MapsterObjectMapper` (`IObjectMapper`) | Обёртка над Mapster `TypeAdapterConfig`; `Map`, in-place `Map`, `ProjectTo` |
| `IMapsterMappingProfile` | Точка расширения: `Configure(TypeAdapterConfig)` для кастомных правил |
| `CrmMapsterModule` | Модуль: регистрирует глобальный `TypeAdapterConfig` (Singleton) и `IObjectMapper` (Scoped), применяя все профили |
| `AddMapping<TProfile>()` | Хелпер регистрации профиля |

## Подключение

```csharp
[DependsOn(typeof(CrmMapsterModule))]
public partial class MyApiModule : CrmModule { }
```

Кастомные правила — через профиль:

```csharp
public class OrderMappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<Order, OrderViewModel>()
              .Map(d => d.CustomerName, s => s.Customer.Name);
    }
}

// регистрация
services.AddMapping<OrderMappingProfile>();
```

Все зарегистрированные `IMapsterMappingProfile` применяются к конфигурации при создании `IObjectMapper`. Для маппинга на стороне БД используйте `ProjectTo<TDest>(IQueryable)` (см. также [Cheetah.Core.Grid](../Cheetah.Core.Grid/README.md)).
