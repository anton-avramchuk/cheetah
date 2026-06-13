# Cheetah.Backend.Jwt.Abstractions

Контракты генерации JWT — без привязки к реализации и ASP.NET. Позволяют модулям (например, Identity) генерировать токены, не завися от конкретной обвязки JwtBearer. Реализация — [Cheetah.Backend.Jwt](../Cheetah.Backend.Jwt/README.md). Зависимостей нет.

## Состав

| Тип | Назначение |
|-----|------------|
| `IJwtTokenGenerator` | `GenerateToken(userId, userName, email, roles, additionalClaims?)` |
| `TokenGenerationResult` | `record (string Token, int ExpiresInSeconds)` |

## Использование

```csharp
public class LoginCommandHandler(IJwtTokenGenerator tokens)
{
    public async ValueTask<TokenGenerationResult> HandleAsync(LoginCommand cmd, CancellationToken ct)
    {
        var user = /* ... */;
        return tokens.GenerateToken(user.Id, user.UserName, user.Email, user.Roles);
    }
}
```

Зависимость на этот пакет (а не на `Cheetah.Backend.Jwt`) держит Application-слой свободным от инфраструктуры аутентификации.
