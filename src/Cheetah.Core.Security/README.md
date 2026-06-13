# Cheetah.Core.Security

Работа с текущим пользователем и claims. Абстракции доступа к `ClaimsPrincipal` и фабрика principal'ов. Зависит только от `Cheetah.Core`. Сервисы регистрируются автоматически через `[Export]`.

## Состав

| Тип | Назначение |
|-----|------------|
| `ICurrentPrincipalAccessor` | Текущий `ClaimsPrincipal`; `Change(principal)` — временная подмена (`IDisposable`, удобно для фоновых задач/тестов) |
| `CurrentPrincipalAccessorBase` | База для реализаций (HTTP, фоновый контекст) |
| `IApplicationClaimsPrincipalFactory` | Сборка обогащённого `ClaimsPrincipal` (через contributor'ы) |
| `ApplicationClaimsPrincipalFactory` | Реализация фабрики |
| `ApplicationClaimsPrincipalContributorContext` | Контекст для contributor'ов, добавляющих claims |
| `ApplicationClaimTypes` | Настраиваемые имена стандартных claim-типов: `UserId`, `UserName`, `Role`, `Email`, `ClientId` и т.д. |
| `ClaimsIdentityExtensions` | Хелперы извлечения значений из claims |

## Использование

```csharp
public class CurrentUserService(ICurrentPrincipalAccessor accessor)
{
    public Guid? UserId =>
        accessor.Principal.FindUserId(); // через ClaimsIdentityExtensions
}
```

Временная подмена principal'а (например, выполнить участок от имени системного пользователя):

```csharp
using (accessor.Change(systemPrincipal))
{
    await DoWorkAsync(); // внутри Principal == systemPrincipal
}
```

Имена claim-типов настраиваются статически через `ApplicationClaimTypes` — например, если внешний провайдер кладёт user id в нестандартный claim.
