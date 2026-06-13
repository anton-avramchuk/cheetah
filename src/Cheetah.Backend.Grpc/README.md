# Cheetah.Backend.Grpc

gRPC-транспорт для Cheetah — встаёт рядом с REST поверх того же `IDispatcher` (CQRS).
Бизнес-логика не дублируется: gRPC-сервисы тонкие и лишь транслируют proto-вызовы в команды/запросы.

## Что предоставляет

- **`CrmBackendGrpcModule`** — подключает серверную инфраструктуру `Grpc.AspNetCore` (`AddGrpc`)
  и регистрирует `CrmExceptionInterceptor`.
- **`CrmExceptionInterceptor`** — транслирует исключения в `RpcException` с корректным `StatusCode`
  (gRPC-аналог `ValidationExceptionHandler` для REST):

  | Исключение | StatusCode |
  |---|---|
  | `EntityNotFoundException` | `NotFound` |
  | `ArgumentException` | `InvalidArgument` |
  | `FormatException`, `OverflowException` | `InvalidArgument` (кривые данные клиента, напр. `Guid.Parse`) |
  | `OperationCanceledException` | `Cancelled` |
  | прочее | `Internal` (логируется как ошибка сервера) |

- **Классы-маркеры** (`Abstractions/`) для gRPC-генератора: `GrpcCommand`, `GrpcCommandWithResult`,
  `GrpcQuery`, `GrpcQueryOrNotFound`. По ним `Cheetah.Generators.Grpc` создаёт реализации сервисов.

## Зависимости

`CoreModule`, `CrmCQRSCoreModule`, `CrmDomainModule`, `CrmAspNetCoreModule`.
**Не зависит от конкретного маппера** — сгенерированные сервисы используют абстракцию `IObjectMapper`,
реализацию выбирает само приложение (Mapster или самописный).

## Как подключить gRPC в модуле

1. **Бутстраппер модуля:** `[DependsOn(typeof(CrmBackendGrpcModule))]`.
2. **`.proto`-контракт** в проекте Api + в `.csproj`:
   ```xml
   <ItemGroup>
     <Protobuf Include="Protos\my.proto" GrpcServices="Server" />
   </ItemGroup>
   <ItemGroup>
     <PackageReference Include="Grpc.Tools"><PrivateAssets>all</PrivateAssets></PackageReference>
   </ItemGroup>
   ```
3. **Классы-маркеры** — связывают proto-методы с CQRS:
   ```csharp
   public sealed class GetByIdGrpcMethod
       : GrpcQueryOrNotFound<GetByIdProtoRequest, GetByIdQuery, MyModel?, MyProtoReply>;
   ```
   Генератор создаст `{ServiceContainer}Service` (наследник proto-базы) с вызовом `IDispatcher`.
4. **Регистратор** (пишется вручную — генераторы не видят вывод друг друга, поэтому `[Export]`
   на сгенерированном классе не подхватился бы):
   ```csharp
   [Export(LifetimeType.Singleton, typeof(IModuleTransportRegistrar))]
   public sealed class MyGrpcRegistrar : IModuleTransportRegistrar
   {
       public void Register(IEndpointRouteBuilder rb, IServiceProvider sp)
           => rb.MapGrpcService<MyServiceService>();
   }
   ```
   `CrmAspNetCoreModule` на старте находит все `IModuleTransportRegistrar` и вызывает их.
   Сам gRPC-сервис в DI регистрировать не нужно — `MapGrpcService<T>()` создаёт его через DI.
5. **Kestrel HTTP/2.** gRPC работает только поверх HTTP/2:
   ```json
   { "Kestrel": { "EndpointDefaults": { "Protocols": "Http1AndHttp2" } } }
   ```
   (REST по HTTP/1.1, gRPC по h2c prior-knowledge на том же порту; под TLS — ALPN автоматически.)

## Маппинг proto ↔ CQRS

Сгенерированный сервис делает `mapper.Map<...>`. proto-типы непростые (`Guid<->string`, optional,
well-known types), поэтому нужна поддержка в мапере:

- **Mapster:** opt-in модуль `Cheetah.Backend.Grpc.Mapster` (`CrmBackendGrpcMapsterModule`) + правила
  в профиле модуля.
- **Самописный маппер:** opt-in модуль `Cheetah.Mapping.Expressions.Protobuf`.

## Архитектурные заметки

- Почему `IModuleTransportRegistrar`, а не `OnApplicationInitialization`-override: REST-генератор уже
  эмитит override модуля; второй override недопустим (CS0111). Реестр транспортов через DI разводит
  REST и gRPC без конфликта генераторов.
