# Этап 9. Пилот и сквозная верификация

> [← Бэклог](README.md) · Зависимости: этапы 5–7.

<a id="t-91"></a>
## T-9.1 Пилот: перевод Catalog в tenant-based — **M**

**Зависит от:** этапы 5–7.
**Файлы:** `Modules/Catalog` → Infrastructure/Default-сборка.

**Шаги:**
1. В Infrastructure (или Default): `services.AddTenantBasedDbContext<CatalogDbContext>("Catalog")`
   ([T-6.1](stage-6-provisioning.md#t-61)). Регистрация безусловная: в безтенантном хосте
   провайдер просто никем не опрашивается, поведение прежнее — проверить, что это
   действительно так (никаких if-ов в модуле).
2. Убедиться, что кэш цен Catalog идёт через нормализатор [T-8.2](stage-8-background.md#t-82).
3. Прогнать существующие 39 тестов Catalog без изменений — они должны остаться зелёными
   (докажет «модуль не знает о тенантности»).

**DoD:** diff в Catalog — считанные строки; все старые тесты зелёные.

<a id="t-92"></a>
## T-9.2 Интеграционный тест: два тенанта, полная изоляция — **L**

**Зависит от:** T-9.1.
**Проект:** новый `Cheetah.Multitenancy.Integration.Tests` (по образцу
`Cheetah.Core.Dapper.Integration.Tests` — реальный PostgreSQL из фикстуры).

**Сценарии (один хост с Tenancy + Catalog + Redis-шиной):**
1. `CreateTenantCommand("Acme","acme")` + `("Globex","globex")` → в PG появились
   `t_acme_catalog`, `t_globex_catalog`; провижининг-статусы `Provisioned`; тенанты `Active`.
2. `Change(acme)` → создать продукт; `Change(globex)` → продуктов нет; в `t_acme_catalog`
   строка есть, в `t_globex_catalog` — нет (прямой SQL-ассерт).
3. Событие `ProductCreatedEvent` (или аналог) от acme несёт `TenantId=acme`; подписчик
   выполнился в контексте acme.
4. Кэш: продукт/цена acme не видна из globex.
5. Строки в `TenantConnectionStrings` зашифрованы (значение в БД не содержит `Host=`/`Database=`
   открытым текстом), `Decrypt` через сервис возвращает валидную строку.
6. Реконсиляция: удалить `t_globex_catalog` руками → рестарт реконсайлера → БД пересоздана.
7. Middleware-срез (`WebApplicationFactory`): запрос с JWT acme-юзера читает данные acme;
   подмена `X-Tenant-Id: globex` при том же JWT — игнорируется.

**DoD:** все 7 сценариев зелёные в CI (или помечены как integration-категория, если CI
без Docker — согласовать с текущей практикой integration-тестов репо).

<a id="t-93"></a>
## T-9.3 Регресс безтенантного режима — **S**

**Зависит от:** T-9.1.

**Шаги:**
1. Хост БЕЗ `CrmTenantsRuntimeModule` и без модуля Tenancy: вся существующая тестовая
   матрица солюшна зелёная (это почти бесплатно — CI уже гоняет).
2. Точечный тест: `DefaultConnectionStringResolver` активен; `ICurrentTenant.Id == null`;
   outbox-поллер делает только хост-проход; кэш-ключи без префикса.
3. Явный smoke: Catalog в безтенантном хосте работает по строке из конфигурации.

**DoD:** формально зафиксировано, что режим «как сейчас» не деградировал.

<a id="t-94"></a>
## T-9.4 Документация — **S**

**Зависит от:** всё выше.

**Шаги:**
1. README новых/изменённых базовых модулей (чек-лист CLAUDE.md): `Core.Tenants`,
   `Core.EntityFramework.Tenants`, `Core.Security` (шифрование), `Core.DataAccess`
   (атрибут), `AspNetCore.Tenants` (новый), шины, Outbox, Cache.
2. `docs/multi-tenancy.md`: перевести из «черновик» в «реализовано», расхождения — поправить.
3. Гайд «как сделать модуль мультитенантным» (одна строка регистрации + чек-лист: кэш через
   нормализатор, события через шину, фоновые джобы через тенант-итерацию) — раздел в
   README `Core.Tenants`, ссылка из корневого CLAUDE.md (раздел Current Modules/архитектура).

**DoD:** новый разработчик может подключить модуль к мультитенантности только по README.
