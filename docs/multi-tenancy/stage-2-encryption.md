# Этап 2. Шифрование (`Cheetah.Core.Security`)

> [← Бэклог](README.md) · Зависимости: нет. Нужен этапу 4 (модуль Tenancy шифрует строки подключения).

<a id="t-21"></a>
## T-2.1 `IStringEncryptionService` (AES-256-GCM, версионируемые ключи) — **M**

**Зависит от:** ничего.
**Файлы:** `src/Cheetah.Core.Security/Encryption/` → `IStringEncryptionService.cs`,
`AesGcmStringEncryptionService.cs`, `StringEncryptionOptions.cs`; тесты.

**Контракт:**
```csharp
public interface IStringEncryptionService
{
    string Encrypt(string plainText);           // активным ключом
    string Decrypt(string cipherText);          // ключом из самого значения
    string ActiveKeyId { get; }
}
```

**Формат значения:** `v1:{keyId}:{base64(nonce)}:{base64(ciphertext||tag)}`.
`v1` — версия формата (задел на смену алгоритма), `keyId` — версия ключа.

**Шаги:**
1. `StringEncryptionOptions` (секция `Security:StringEncryption`):
   `Keys: Dictionary<string, string>` (keyId → base64 32-байтового ключа), `ActiveKeyId`.
   Валидация при старте: активный ключ существует, длина 32 байта — иначе понятный
   `CrmException` (fail-fast, а не при первом Encrypt).
2. Реализация на `System.Security.Cryptography.AesGcm`: nonce 12 байт
   (`RandomNumberGenerator`), tag 16 байт. Никаких статических nonce/IV.
3. `Decrypt`: распарсить формат → найти ключ по `keyId` (может быть НЕ активным — это
   и есть поддержка ротации: старые значения читаются старым ключом, новые пишутся новым);
   неизвестный `keyId` → `CrmException` с внятным сообщением.
4. Регистрация `[Export(LifetimeType.Singleton, ...)]` в модуле Security.
5. Юнит-тесты: roundtrip; два Encrypt одного текста дают разные значения (nonce);
   подмена одного байта ciphertext → `CryptographicException`; чтение значения,
   зашифрованного не-активным ключом; неизвестный keyId → ошибка; невалидная
   конфигурация → ошибка при старте.

**DoD:** сервис + ≥6 тестов + README-раздел (как сгенерировать ключ:
`openssl rand -base64 32`, как ротировать: добавить ключ → сменить `ActiveKeyId` →
фоновая перешифровка — команда появится в [T-4.5](stage-4-tenancy-module.md#t-45)).
