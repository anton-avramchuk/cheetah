# Cheetah.Backend.Grpc.Mapster

Opt-in модуль: добавляет Mapster-правила конвертации между .NET-типами и proto well-known types.
Нужен приложениям, которые используют **gRPC вместе с Mapster** (`CrmMapsterModule`).

## Что внутри

- **`CrmBackendGrpcMapsterModule`** — регистрирует `ProtoMappingProfile`.
- **`ProtoMappingProfile`** (`IMapsterMappingProfile`) — конверсии:
  `DateTime`/`DateTimeOffset` ↔ `Google.Protobuf.WellKnownTypes.Timestamp`.

Специфичные для модуля правила (`Guid <-> string` для конкретного `GuidProto`, обработка `optional`
полей и т.п.) задаются в `MappingProfile` соответствующего бизнес-модуля.

## Зависимости

`CoreModule`, `CrmMapsterModule`. Тянет пакет `Google.Protobuf`.

## Подключение

```csharp
[DependsOn(typeof(CrmBackendGrpcModule))]
[DependsOn(typeof(CrmBackendGrpcMapsterModule))]
public partial class MyBootstrapperModule : CrmModule { ... }
```

## Почему вынесено отдельно

`Cheetah.Backend.Grpc` намеренно не зависит ни от Mapster, ни от Google.Protobuf — транспорт работает
через абстракцию `IObjectMapper`. Mapster-специфичная proto-склейка живёт здесь, чтобы приложения на
самописном мапере (`CrmCustomMappingModule` + `Cheetah.Mapping.Expressions.Protobuf`) её не тянули.
