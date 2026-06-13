# Cheetah.Mapping.Expressions.Protobuf

Opt-in расширение самописного маппера (`Cheetah.Mapping.Expressions` / `ExpressionObjectMapper`)
поддержкой proto-типов. Нужен приложениям, которые используют **gRPC вместе с самописным маппером**
(`CrmCustomMappingModule`). Аналог `Cheetah.Backend.Grpc.Mapster` для Mapster.

## Что регистрирует

Через точки расширения `ExpressionBuilder` (см. README ядра маппера):

- Конверсии `DateTime`/`DateTimeOffset` ↔ `Google.Protobuf.WellKnownTypes.Timestamp`
  (`ExpressionBuilder.RegisterConverter`).
- Предикат «proto-сообщение» (`typeof(IMessage).IsAssignableFrom`) для null-safe сборки
  (`ExpressionBuilder.RegisterNullSafeDestinationType`): ссылочные proto-поля не получают `null`
  (proto-сеттер строки кидает на `null`), а скаляр можно завернуть в одно-полевое сообщение.

`Guid <-> string` и маппинг через конструктор (позиционные record'ы) встроены в само ядро маппера
и proto-расширения не требуют.

## Зависимости

`CoreModule`, `CrmCustomMappingModule`. Тянет пакет `Google.Protobuf`.

## Подключение

```csharp
[DependsOn(typeof(CrmCustomMappingModule))]          // самописный маппер
[DependsOn(typeof(CrmCustomMappingProtobufModule))]  // + proto-поддержка
public partial class MyBootstrapperModule : CrmModule { ... }
```

`CrmCustomMappingProtobufModule.ConfigureServices` вызывает `ProtobufMappingSupport.Register()`
(идемпотентно). Регистрация глобальная и статическая — выполняется один раз на старте, до первых маппингов.
