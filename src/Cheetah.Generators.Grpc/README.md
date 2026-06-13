# Cheetah.Generators.Grpc

Incremental source generator: по классам-маркерам создаёт реализации gRPC-сервисов, транслирующие
proto-вызовы в `IDispatcher` (CQRS). gRPC-аналог `Cheetah.Generators.Endpoints` для REST.

Подключается как анализатор:
```xml
<ProjectReference Include="..\..\..\Cheetah.Generators.Grpc\Cheetah.Generators.Grpc.csproj"
                  ReferenceOutputAssembly="false" OutputItemType="Analyzer" />
```

## Классы-маркеры (из `Cheetah.Backend.Grpc`)

| Маркер | Тело сгенерированного метода |
|---|---|
| `GrpcCommand<TProtoReq, TCommand>` | `Map -> SendAsync -> return new TProtoResp()` |
| `GrpcCommandWithResult<TProtoReq, TCommand, TResult, TProtoResp>` | `Map -> SendAsync<,> -> Map(result)` |
| `GrpcQuery<TProtoReq, TQuery, TResult, TProtoResp>` | `Map -> QueryAsync<,> -> Map(result)` |
| `GrpcQueryOrNotFound<TProtoReq, TQuery, TResult, TProtoResp>` | то же + `null -> RpcException(NotFound)` |

## Как работает

1. Находит классы-наследники маркеров, достаёт их generic-параметры.
2. Сканирует методы proto-базовых классов в сборке модуля (методы вида
   `(TProtoRequest, ServerCallContext) -> Task<TResp>`) и **сопоставляет маркер с rpc-методом по типу
   proto-запроса** (`param[0]`). Имя метода, сигнатуру и тип ответа берёт из найденного метода.
3. Группирует маркеры по proto-базе и эмитит один класс `{ServiceContainer}Service` на сервис
   (например proto `service CustomerGrpc` → `CustomerGrpcService : CustomerGrpc.CustomerGrpcBase`).

Сгенерированный сервис инжектит `IDispatcher` + `IObjectMapper`; в DI его регистрировать не нужно —
`MapGrpcService<T>()` создаёт его через DI.

## Что НЕ генерируется (пишется вручную)

- **Регистратор** `IModuleTransportRegistrar` (`MapGrpcService<...>()`): source-генераторы не видят
  вывод друг друга, поэтому `[Export]` на сгенерированном классе не подхватился бы `ModuleServicesGenerator`.
  Регистратор ссылается на сгенерированный сервис по детерминированному имени.

## Диагностики

- **`GRPCGEN001`** (warning) — для маркера не найден proto-метод с таким типом запроса.

## Ограничения

- Сопоставление идёт по типу proto-запроса; если два proto-сервиса имеют методы с одинаковым типом
  запроса — берётся первый найденный (в реальных proto типы запросов уникальны).

## Тесты

`Cheetah.Generators.Grpc.Tests` — через `CSharpGeneratorDriver` на синтетической компиляции:
форма генерации каждого вида, диагностика `GRPCGEN001`, мультиметодный сервис, чистая компиляция вывода.
