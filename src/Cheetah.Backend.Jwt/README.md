# Cheetah.Backend.Jwt

Готовая JWT-аутентификация для приложения: генерация токенов + настройка `JwtBearer` и authorization middleware. Реализует [Cheetah.Backend.Jwt.Abstractions](../Cheetah.Backend.Jwt.Abstractions/README.md). Зависит от `Cheetah.Core`, `CrmAspNetCoreModule`.

## Состав

| Тип | Назначение |
|-----|------------|
| `CrmBackendJwtModule` | Модуль: биндит `JwtOptions`, регистрирует провайдер ключа по алгоритму, настраивает `AddJwtBearer` + authorization, в `OnApplicationInitialization` подключает `UseAuthentication`/`UseAuthorization` |
| `JwtTokenGenerator` (`IJwtTokenGenerator`) | Генерация подписанного токена (пользовательского и сервисного) |
| `IJwtSigningKeyProvider` | Поставляет ключ подписи и публичные ключи (JWKS). Реализации: `HmacJwtSigningKeyProvider` (HS256), `RsaJwtSigningKeyProvider` (RS256) |
| `JwtOptions` / `JwtOptionsValidator` | Опции секции `Jwt` + условная валидация на старте |

## Алгоритмы подписи

Управляется `Jwt:SigningAlgorithm`:

- **`HS256`** (default) — симметричный секрет `SecretKey`. Подходит для монолита: один процесс и подписывает, и валидирует.
- **`RS256`** — асимметричная подпись. **Подписывает только эмитент** (Identity, держит `PrivateKeyPem`); остальные сервисы валидируют публичным ключом. Это правильный режим для микросервисов — секрет не раскатывается по сервисам.

В режиме RS256 эмитент публикует:
- `GET /.well-known/jwks.json` — публичный ключ (JWK с `kid`);
- `GET /.well-known/openid-configuration` — OIDC discovery с `jwks_uri`.

(эндпоинты маппит `Cheetah.Modules.Identity.Api`, когда подпись асимметричная).

Токены несут `kid` в заголовке → валидатор подбирает ключ по JWKS (готово к ротации).

## Конфигурация

**Монолит (HS256, как было):**
```json
{ "Jwt": { "SecretKey": "<≥32 символов>", "Issuer": "cheetah", "Audience": "cheetah-clients", "ExpirationMinutes": 60 } }
```

**Эмитент в RS256 (Identity):**
```json
{ "Jwt": {
  "SigningAlgorithm": "RS256",
  "PrivateKeyPem": "-----BEGIN PRIVATE KEY-----\n...",
  "KeyId": "",                       // пусто → отпечаток публичного ключа
  "Issuer": "cheetah", "Audience": "cheetah-clients"
} }
```

**Сервис-валидатор в RS256 (ключей в конфиге нет — берёт по JWKS):**
```json
{ "Jwt": {
  "SigningAlgorithm": "RS256",
  "MetadataAddress": "https://identity/.well-known/openid-configuration",
  "RequireHttpsMetadata": true,
  "Issuer": "cheetah", "Audience": "cheetah-clients"
} }
```

Опции валидируются на старте (`JwtOptionsValidator`): для HS256 нужен `SecretKey`≥32; для RS256-эмитента — `PrivateKeyPem`; валидатор с `MetadataAddress` ключей не требует.

## Поведение

- Валидация токена: issuer, audience, lifetime, signing key; `ClockSkew = 0`.
- Локальный режим — ключ берётся у `IJwtSigningKeyProvider` (HMAC или публичный RSA). При заданном `MetadataAddress` — ключи тянутся из JWKS эмитента (кэш/ротация средствами `JwtBearer`).
- Глобальный `FallbackPolicy` требует аутентифицированного пользователя — все endpoint'ы защищены по умолчанию; публичные помечаются `AllowAnonymous` (см. [Cheetah.Backend.Endpoints](../Cheetah.Backend.Endpoints/README.md)).

> ⚠️ Сервисные токены (`token_type=service`, см. [Cheetah.Backend.ServiceAuth](../Cheetah.Backend.ServiceAuth/README.md)) сейчас проходят `FallbackPolicy` как обычный пользователь. Разграничение authz (скоупы/политики user vs service) — следующий шаг.

## Подключение

```csharp
[DependsOn(typeof(CrmBackendJwtModule))]
public partial class MyAppModule : CrmModule { }
```
