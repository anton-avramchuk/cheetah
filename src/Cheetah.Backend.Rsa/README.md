# Cheetah.Backend.Rsa

Модуль RSA-шифрования паролей для Cheetah. Позволяет Angular-фронтенду шифровать пароль публичным ключом перед отправкой на сервер, а серверу — расшифровывать его приватным ключом перед проверкой через `UserManager`.

Используется как опциональная зависимость модуля `Cheetah.Modules.Identity.Api`: если `CrmBackendRsaModule` подключён в хосте, шифрование активируется автоматически без изменений кода.

## Состав

| Проект | Назначение |
|--------|-----------|
| `Cheetah.Backend.Rsa.Abstractions` | Интерфейсы `IPasswordDecryptor` и `IRsaPublicKeyProvider`. Зависимостей нет. |
| `Cheetah.Backend.Rsa` | Реализация `RsaPasswordDecryptor`, модуль `CrmBackendRsaModule`. |

## Конфигурация

Добавьте в `appsettings.json`:

```json
{
  "Rsa": {
    "PrivateKeyPem": "-----BEGIN RSA PRIVATE KEY-----\n...\n-----END RSA PRIVATE KEY-----"
  }
}
```

Параметр `PrivateKeyPem` обязателен — приложение упадёт при старте (`ValidateOnStart`), если он не задан.

### Генерация ключей

```bash
# Сгенерировать приватный ключ RSA-2048
openssl genrsa -out private.pem 2048

# Просмотреть публичный ключ (для справки — на сервере он извлекается автоматически)
openssl rsa -in private.pem -pubout
```

Содержимое `private.pem` целиком вставляется в конфигурацию. В `appsettings.json` переносы строк заменяются на `\n`. В `appsettings.Production.json` (или secrets) рекомендуется хранить PEM как есть.

## Подключение

Добавьте `CrmBackendRsaModule` в `[DependsOn]` вашего хост-модуля:

```csharp
[DependsOn(
    typeof(CheetahIdentityApiModule),
    typeof(CrmBackendRsaModule)   // <-- активирует RSA
)]
public partial class MyAppModule : CrmModule { }
```

После этого:
- `IPasswordDecryptor` в DI разрешается в `RsaPasswordDecryptor` (вместо pass-through заглушки из Identity.Application).
- Автоматически регистрируется эндпоинт `GET /api/auth/public-key`, отдающий публичный ключ фронтенду.

## Интеграция с Angular

Фронтенд при инициализации приложения запрашивает публичный ключ и использует его для шифрования паролей:

```typescript
// auth.service.ts
async getPublicKey(): Promise<CryptoKey> {
  const res = await fetch('/api/auth/public-key');
  const { publicKey } = await res.json();

  const keyBytes = Uint8Array.from(atob(publicKey), c => c.charCodeAt(0));
  return crypto.subtle.importKey(
    'spki',
    keyBytes,
    { name: 'RSA-OAEP', hash: 'SHA-256' },
    false,
    ['encrypt']
  );
}

async encryptPassword(publicKey: CryptoKey, password: string): Promise<string> {
  const encoded = new TextEncoder().encode(password);
  const encrypted = await crypto.subtle.encrypt({ name: 'RSA-OAEP' }, publicKey, encoded);
  return btoa(String.fromCharCode(...new Uint8Array(encrypted)));
}
```

Зашифрованная строка в base64 передаётся в поле `password` при логине.

> Публичный ключ можно закэшировать в `environment.ts` на этапе сборки или в `APP_INITIALIZER` — сетевой запрос происходит один раз.

## Как это работает

```
Angular (SubtleCrypto)           ASP.NET Core (Cheetah)
─────────────────────────────    ──────────────────────────────────────────
password ──RSA-OAEP-SHA256──►   encrypted_base64
                                    │
                              IPasswordDecryptor.Decrypt()
                                    │ RSA.Decrypt(OaepSHA256)
                                    ▼
                              plain_password
                                    │
                              UserManager.CheckPasswordAsync()
```

Если `CrmBackendRsaModule` не подключён, `IPasswordDecryptor` разрешается в `PassThroughPasswordDecryptor` (no-op), и пароль передаётся в `UserManager` без изменений.

## Регистрация в DI

`RsaPasswordDecryptor` реализует оба интерфейса и регистрируется как **один singleton**:

```csharp
services.AddSingleton<RsaPasswordDecryptor>();
services.AddSingleton<IPasswordDecryptor>(sp => sp.GetRequiredService<RsaPasswordDecryptor>());
services.AddSingleton<IRsaPublicKeyProvider>(sp => sp.GetRequiredService<RsaPasswordDecryptor>());
```

Это гарантирует единственный экземпляр RSA-ключа на весь lifetime приложения.

## Архитектура

```
Cheetah.Backend.Rsa.Abstractions
  IPasswordDecryptor      — расшифровать пароль
  IRsaPublicKeyProvider   — получить публичный ключ в base64 (SPKI)

Cheetah.Backend.Rsa
  RsaPasswordDecryptor    — реализует оба интерфейса, RSA-OAEP-SHA256
  RsaOptions              — конфигурация (PrivateKeyPem)
  CrmBackendRsaModule     — модуль, регистрирует сервисы и options
```

## Зависимости

- `Cheetah.Core` — базовая инфраструктура модулей
- `Microsoft.AspNetCore.App` — `IOptions<T>`, `ValidateDataAnnotations`
- `System.Security.Cryptography` — встроен в .NET

## Тестирование

Смотрите `Cheetah.Backend.Rsa.Tests/RsaPasswordDecryptorTests.cs`.

Покрытые сценарии:
- Round-trip шифрование/расшифровка для стандартных и unicode-паролей
- Невалидный base64 → `FormatException`
- Данные зашифрованы другим ключом → `CryptographicException`
- `PublicKeyBase64` не пустой и содержит валидный SPKI-ключ
- Публичный ключ из `PublicKeyBase64` соответствует приватному (round-trip через импорт)
