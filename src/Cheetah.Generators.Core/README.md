# Cheetah.Generators.Core

Общая инфраструктура для всех source-генераторов Cheetah. Не содержит генераторов сам по себе — это вспомогательная библиотека, на которую ссылаются остальные `Cheetah.Generators.*`.

## Состав

| Тип | Назначение |
|-----|------------|
| `Constants` | Полные имена ключевых типов фреймворка для поиска в семантической модели: `BootstrapperAttributeName`, `ModuleTypeName` (`ICrmModule`), `ExportAttributeName`, `DependsOnAttributeName` |
| `IsExternalInit` | Полифилл для `init`-сеттеров/record'ов в `netstandard2.0`-сборке генератора |

## Зачем

Source-генераторы компилируются под `netstandard2.0` и работают на уровне символов Roslyn. `Constants` централизует строковые имена атрибутов/интерфейсов, по которым генераторы находят модули, `[Export]`-сервисы и зависимости — чтобы при переименовании в `Cheetah.Core` правка была в одном месте. `IsExternalInit` даёт доступ к современному синтаксису C# в таргете, где его нет в BCL.

Используется генераторами [Module](../Cheetah.Generators.Module/README.md), [Application](../Cheetah.Generators.Application/README.md), [Endpoints](../Cheetah.Generators.Endpoints/README.md), [ApiClient](../Cheetah.Generators.ApiClient/README.md), [Grpc](../Cheetah.Generators.Grpc/README.md), [Common](../Cheetah.Generators.Common/README.md).
