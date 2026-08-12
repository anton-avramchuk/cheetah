# Cheetah.Scalar

Интерактивный UI документации API на базе **Scalar** поверх документа [Cheetah.OpenApi](../Cheetah.OpenApi/README.md). Добавляет схему авторизации Bearer (JWT) в документ и применяет её ко всем защищённым операциям. Зависит от `Cheetah.Core`, `CrmAspNetCoreModule`, `OpenApiModule`.

## Состав

| Тип | Назначение |
|-----|------------|
| `ScalarModule` | Регистрирует Bearer security scheme + requirement, в `OnApplicationInitialization` маппит Scalar UI (anonymous) |
| `ScalarModuleOptions` | `OpenApiPath` — путь к OpenAPI-документу для UI |

## Поведение

- В OpenAPI-документ добавляется security scheme `Bearer` (HTTP `bearer`, формат JWT).
- Ко всем операциям, **не** помеченным `AllowAnonymous`, применяется требование Bearer — в UI появляется поле для ввода токена.
- Scalar UI и `/openapi` отдаются анонимно.

## Подключение

```csharp
[DependsOn(typeof(ScalarModule))]
public partial class MyAppModule : CrmModule { }
```

```csharp
// при необходимости указать нестандартный путь к документу
services.Configure<ScalarModuleOptions>(o => o.OpenApiPath = "/openapi/v1.json");
```

По умолчанию путь собирается из имени документа (`OpenApi:DocumentName`), так что для нестандартного
имени достаточно конфигурации:

```json
{ "OpenApi": { "DocumentName": "crm" }, "Scalar": { "SecurityScheme": "None" } }
```

`Scalar:SecurityScheme: "None"` убирает Bearer и из документа, и из UI — это для сервиса, который
пускает не по токену. У BFF, например, cookie-сессия: объявленный Bearer означал бы в спеке и на
странице поле для токена, которого у него не бывает.

Связка авторизации (`AllowAnonymous` на endpoint'ах) согласована с [Cheetah.Backend.Jwt](../Cheetah.Backend.Jwt/README.md) и [Cheetah.Backend.Endpoints](../Cheetah.Backend.Endpoints/README.md).
