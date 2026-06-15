# Cheetah.Modules.Deals.Client

HTTP-клиент к Deals.Api для server-to-server интеграции (например, конвертация Lead → Deal).
Зависит на `Cheetah.Core` + `…Contracts`.

## API

```csharp
public interface IDealsClient
{
    ValueTask<Guid> CreateDealAsync(CreateDealRequest request, CancellationToken ct = default);
    ValueTask<DealDto?> GetByIdAsync(Guid dealId, CancellationToken ct = default); // null на 404
}
```

## Подключение

```csharp
[DependsOn(typeof(CheetahDealsClientModule))]
public class MyModule : CrmModule { }
```

Опции секции `Deals:Client` (валидируются на старте): `BaseUrl` (обязателен, абсолютный URI),
`Timeout` (по умолчанию 5 c). `HttpDealsClient` регистрируется через `AddHttpClient`.

## Тесты

`Client.Tests` — на `HttpMessageHandler`-стабе: парсинг `Created`-ответа, `null` на 404, исключение
на 5xx.
