# Cheetah.Backend.Rsa.Abstractions

Контракты RSA-шифрования паролей — без привязки к реализации. Используются там, где клиент шифрует пароль публичным ключом, а сервер расшифровывает приватным. Реализация — [Cheetah.Backend.Rsa](../Cheetah.Backend.Rsa/README.md). Зависимостей нет.

## Состав

| Тип | Назначение |
|-----|------------|
| `IRsaPublicKeyProvider` | `PublicKeyBase64` — публичный ключ для отдачи фронтенду |
| `IPasswordDecryptor` | `Decrypt(encryptedPassword)` — расшифровка пришедшего пароля |

## Сценарий

1. Фронтенд запрашивает публичный ключ (`IRsaPublicKeyProvider.PublicKeyBase64`).
2. Шифрует пароль перед отправкой.
3. Сервер в хендлере логина расшифровывает: `IPasswordDecryptor.Decrypt(request.Password)`.

Зависимость на абстракции (а не на `Cheetah.Backend.Rsa`) держит Application-слой свободным от криптографической реализации.
