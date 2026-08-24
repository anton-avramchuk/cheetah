# Аудит Cheetah CRM

> Системное ревью репозитория. Ветка `dev`, коммит `9e56753`, 22 августа 2026.
> 145 проектов, 19 бизнес-модулей, ~100 000 строк C#.
> Сборка и тесты запускались фактически; поведение HTTP-биндинга проверялось на живых
> тестовых minimal API, а не по документации.

| Показатель | Значение |
|---|---|
| Сборка | 0 ошибок, 22 предупреждения (уязвимые транзитивные пакеты) |
| Юнит-тесты | 1570 в 78 проектах, 0 провалов |
| Модули | 19 (один без единого теста) |
| Находки | 15 — 4 высоких, 7 средних, 4 низких |

**Не входило в объём:** integration-тесты (нужны Docker и живые БД), нагрузочные измерения,
построчное ревью Blazor-UI и SQL-миграций.

---

## Высокий риск

### 1. Генератор эндпоинтов игнорирует `RequirePermissions` — безопасность

`EndpointConfiguration.RequirePermissions("orders.create")` компилируется, попадает в
`IEndpointDefinition.RequiredPermissions` — и на этом всё. Генератор регистрации читает
`AllowAnonymous` и `AuthorizationPolicies`, но `RequiredPermissions` не применяет нигде.

Провал тихий: эндпоинт остаётся полностью открытым, ни ошибки компиляции, ни предупреждения.
При этом README и EXAMPLES базового модуля рекламируют этот вызов как штатный способ защиты —
то есть первый, кто последует документации, получит незащищённый маршрут и будет уверен в обратном.

- `src/Cheetah.Generators.Endpoints/EndpointRegistrationGenerator.cs:807`
- `src/Cheetah.Backend.Endpoints/Configuration/EndpointConfiguration.cs:56`
- `src/Cheetah.Backend.Endpoints/README.md:37`, `EXAMPLES.md:106`

**Чинить:** в генераторе добавить `builder.RequirePermission(p)` для каждого требования
(extension уже есть в `Cheetah.Permissions`) — либо, если связывать генератор с модулем Permissions
нежелательно, убрать `RequirePermissions` из API и документации, чтобы он не выглядел работающим.

### 2. Ни один бизнес-модуль не требует авторизации — безопасность

Из 19 Api-сборок авторизацию применяет только `Cheetah.Permissions.Catalog.Api`. Сделки, каталог,
клиенты, команды, заметки, документы продаж, workflow — все маршруты анонимны на уровне шаблона.

Формально это ответственность приложения-хоста (хоста в репозитории нет), и в README отдельных
модулей ограничение упомянуто. Но по факту любой, кто соберёт хост из готовых модулей и не настроит
fallback-политику вручную, получит открытый наружу CRM. Для набора шаблонов, продаваемого как
«рабочий CRUD из коробки», это неверный дефолт.

**Чинить:** назначить в шаблонах эндпоинтов явные требования (после исправления №1) и/или
зафиксировать в базовом модуле хоста `FallbackPolicy = RequireAuthenticatedUser`, с явным
`AllowAnonymousAccess()` там, где анонимность нужна.

### 3. `pageSize=0` отдаёт таблицу целиком — устойчивость

В `GridRequest` нулевой размер страницы задокументирован как «без пагинации», а `EfGridRepository`
при `PageSize <= 0` просто не применяет `Skip/Take`. Значение приходит прямо из query-строки,
верхнего предела тоже нет.

Запрос `?pageSize=0` (или `?pageSize=1000000`) к любому гриду выгружает всю таблицу через проекцию
в память приложения. При цели в 10 000 RPS это готовый способ положить сервис одним curl — и при
этом эндпоинты грида, как и все прочие, анонимны (см. №2).

- `src/Cheetah.Contracts/Requests/GridRequest.cs:16`
- `src/Cheetah.Core.Grid/EfGridRepository.cs:72`

**Чинить:** нормализовать размер страницы в репозитории — `Clamp(1, MaxPageSize)` с потолком
порядка 200–500; «без пагинации» оставить только для внутренних вызовов, не выводя его в биндер.

### 4. `ShutdownAsync()` всегда падает — корректность

`CrmApplicationBase.ShutdownAsync()` — публичный API остановки приложения — вызывает
`ModuleManager.ShutdownModulesAsync`, а там стоит `throw new NotImplementedException()`.
Синхронный `ShutdownModules` при этом реализован полностью.

Хост, выбравший асинхронную остановку (естественный выбор в ASP.NET), падает при завершении:
модули не получают свою фазу shutdown, ресурсы не освобождаются штатно.

- `src/Cheetah.Core/Modularity/ModuleManager.cs:65`
- `src/Cheetah.Core/CrmApplicationBase.cs:329`

**Чинить:** реализовать асинхронный обход контрибьюторов по образцу синхронной версии (она в том же
файле, строка 68) — либо, как минимум, сделать её `Task`-обёрткой над синхронной.

---

## Средний риск

### 5. Обязательные query-параметры ломают списки — корректность

Не-nullable значимый тип в `[FromQuery]` minimal API считает обязательным. В четырёх списочных
эндпоинтах это флаги и пагинация, которые клиент естественно опускает.

Проверено на живом minimal API (net10): `GET /customers` без `?activeOnly=` → **400**,
`GET /rules` без `?page=&size=` → **400**. В Workflow это видно и по самому коду:
`size <= 0 ? 50 : size` написан в расчёте на отсутствие параметра, до которого исполнение не доходит.

- `src/Modules/Customer/…Api/Endpoints/CustomerEndpointsBase.cs:65`, `ContactEndpointsBase.cs:58`
- `src/Modules/Workflow/…Api/Endpoints/AutomationRuleEndpointsBase.cs:51, 96`

**Чинить:** `bool?` / `int?` плюс нормализация в обработчике — ровно как сделано в Notes.
Позиционные записи со значением по умолчанию (`[FromQuery] int Qty = 1` в Catalog) работают
корректно и правки не требуют — проверено.

### 6. Workflow читает журнал прогонов целиком в память — производительность

`ListRunsQueryHandler` вызывает `GetAllAsync(null, ct)`, затем фильтрует по правилу и статусу,
сортирует и режет страницу уже в памяти. Так же устроен `ListRulesQueryHandler`.

`AutomationRun` — журнал: он растёт с каждым срабатыванием правила и не подчищается. Список
прогонов деградирует линейно и в какой-то момент кладёт процесс по памяти; пагинация при этом
не спасает, потому что применяется после материализации.

- `src/Modules/Workflow/…Application/Queries/RuleQueries.cs:53, 68–80`

**Чинить:** перенести фильтр в спецификации (правило + статус), а сортировку и `Skip/Take` —
на сторону БД, как это сделано в `EfNoteReader` у Notes.

### 7. Пагинация без устойчивого порядка — корректность

`EfGridRepository` применяет `Skip/Take` даже когда сортировка не задана вовсе, и ни в одном случае
не добавляет тай-брейкер. `DealRepository.ListAsync` сортирует только по `CreatedAt`.

`CrmDbContext` штампует один и тот же `UtcNow` всем сущностям одного `SaveChanges`, поэтому у
записей, созданных пачкой (импорт, сидинг), метки совпадают. Postgres волен вернуть их в любом
порядке — страницы начинают дублировать одни строки и терять другие. Без `OrderBy` вообще порядок
не определён в принципе.

- `src/Cheetah.Core.Grid/EfGridRepository.cs:68–77`
- `src/Modules/Deals/…Infrastructure/Repositories/DealsRepositories.cs:33`

**Чинить:** в гриде — сортировка по умолчанию плюс финальный `ThenBy(x => x.Id)`;
в Deals — `ThenBy(d => d.Id)`.

### 8. Redis-шина молча теряет события — надёжность

В `CrmRedisEventBus.HandleEventAsync` исключение обработчика ловится пустым `catch` с комментарием
«TODO: Add logging». Ни лога, ни метрики, ни повторной доставки.

Сбойное событие исчезает бесследно — при событийной архитектуре это расхождение состояний между
модулями, которое невозможно ни заметить, ни расследовать. Рядом, в `Subscribe`,
`GetAwaiter().GetResult()` вызывается внутри `lock` — блокирующее ожидание под блокировкой на старте.

- `src/Cheetah.Backend.Events.Redis/CrmRedisEventBus.cs:62, 97–100`
- `src/Cheetah.Backend.Events.InMemory/InMemoryEventBus.cs:95` — то же самое

**Чинить:** как минимум `ILogger` с типом события и обработчика; по-хорошему — маршрут
в dead-letter, раз Inbox/Outbox в проекте уже есть.

### 9. Два проекта выпали из solution — процесс

`Cheetah.Mapping.Expressions.Protobuf` и `Cheetah.Mapping.Expressions.Tests` не перечислены
в `Cheetah.slnx`, хотя лежат в `src/` и полностью рабочие: собрал вручную — 0 предупреждений,
прогнал тесты — 7 зелёных.

CI собирает и тестирует по solution, значит эти 7 тестов не выполняются ни разу, а поломка
в Protobuf-маппере не будет замечена. `publish-nuget.yml` тоже пакует solution — библиотека
не попадает в GitHub Packages, хотя `IsPackable` у неё включён.

**Чинить:** `dotnet sln Cheetah.slnx add` для обоих проектов.

### 10. Identity.Application зависит от Infrastructure — архитектура

Нарушение правила 18 из CLAUDE.md — единственное на весь репозиторий. Причина мелкая: обработчики
команд импортируют типы исключений из `Cheetah.Modules.Identity.Infrastructure.Exceptions`.

Слой приложения оказывается прибит к EF-сборке ради семи `using`-ов; вынести Identity в отдельный
сервис или подменить хранилище нельзя без правки Application.

- `src/Modules/Identity/…Application/Cheetah.Modules.Identity.Application.csproj:9`

**Чинить:** перенести типы исключений в Domain или Application и снять ProjectReference. Это же
снимет давно отложенное переименование `Identity.DataAccess` → `Infrastructure`.

### 11. Модуль Tags без единого теста — покрытие

Восемь сборок, включая доменную логику назначения тегов и сервис синхронизации пользователей, —
и ни одного тестового проекта. Остальные 18 модулей покрыты хотя бы Domain + Application.

Tags — общая для многих модулей точка (полиморфная привязка `EntityType/EntityId`, реестр типов).
Регрессия здесь расходится по всей системе и ловится только руками.

**Чинить:** минимум Domain.Tests на правила назначения и Application.Tests на `AssignTagsCommand` —
по образцу Notes или Teams.

---

## Низкий риск и техдолг

### 12. `ConvertFromString` не поддерживает дробные типы

`TypeHelper.ConvertFromString` для `float`, `double` и `decimal` бросает `NotImplementedException`
вместо конвертации — притом что `TypeDescriptor` строкой ниже справился бы сам. Сейчас никто
не вызывает, но метод публичный.

- `src/Cheetah.Core/Reflection/TypeHelper.cs:324`

### 13. Уязвимые транзитивные пакеты

`SSH.NET 2025.1.0` (High) и `SQLitePCLRaw.lib.e_sqlite3 2.1.11` (High) плюс `AngleSharp 1.4.0`
(Moderate) — 22 предупреждения сборки. Все три приходят транзитивно (Testcontainers,
Sqlite-провайдеры, bUnit), продакшн-путей не задевают, но шумят в каждой сборке и всплывут
в аудите поставки.

**Чинить:** поднять корневые пакеты в `Directory.Packages.props` — известный отложенный пункт,
стоит закрыть, чтобы «0 warnings» снова означало ноль.

### 14. CLAUDE.md противоречит принятому Outbox-паттерну

Правило «Always publish events AFTER `SaveChangesAsync()`» (CLAUDE.md:309) прямо расходится с тем,
как устроены шесть модулей: Notification, Email, Calendar, FeatureManagement и другие публикуют
*до* сохранения, чтобы `OutboxEventBus` записал сообщение в тот же `DbContext` и один коммит
зафиксировал агрегат вместе с outbox-строкой атомарно.

Код прав, правило устарело. В нынешнем виде оно подталкивает новые модули к неатомарной публикации —
ровно к той ошибке, от которой Outbox и защищает.

**Чинить:** переформулировать: «через Outbox — до `SaveChangesAsync` в том же контексте;
при прямой публикации в шину — после».

### 15. Мелочи, оставленные в ядре

Девять `TODO` в продакшн-коде, из них содержательные — логирование в обеих шинах событий (см. №8)
и заготовки в `CrmApplicationBase`. Скелет `Cheetah.Core.Tenants` лежит в репозитории, собирается,
но реализации мультитенантности за ним пока нет — при этом `Cheetah.Core.EntityFramework.Tenants`
уже вызывает `MigrateAllTenantsAsync()` через `GetAwaiter().GetResult()`.

---

## Что проверено и оказалось в порядке

Сканирование всей кодовой базы, а не выборка. Эти правила соблюдаются без единого исключения.

- **Ни одного контроллера** — только minimal API, как требует правило 1.
- **Application нигде не трогает `DbContext`** — все совпадения оказались комментариями,
  объясняющими Outbox.
- **`builder.Ignore(e => e.DomainEvents)`** есть у всех 42 агрегатов — совпадения «без Ignore»
  указывали на файлы `DbContext`, тогда как сам вызов стоит в базовых конфигурациях.
- **README есть у всех 102 базовых модулей** — правило pre-commit чек-листа выполняется.
- **Секретов в конфигурации нет**, SQL в Dapper-слое параметризован, имена таблиц берутся
  из метаданных, а не из пользовательского ввода.
- **Ни одного `async void`** и ни одного `DateTime.Now` в продакшн-коде.
- **Индексы** расставлены во всех значимых EF-конфигурациях.

---

## Покрытие тестами по модулям

Слои, у которых есть свой тестовый проект. Integration-тесты в прогон не входили.

| Модуль | Сборок | Тестовых | Слои |
|---|---:|---:|---|
| Tags | 8 | 0 | **нет тестов** |
| Email | 7 | 1 | Application |
| Permissions | 5 | 1 | Permissions |
| Identity | 12 | 2 | Application, Client |
| Activities | 10 | 2 | Domain, Application |
| Catalog | 13 | 2 | Domain, Application |
| Customer | 9 | 2 | Domain, Application |
| Leads | 10 | 2 | Domain, Application |
| NotesTimeline | 9 | 2 | Domain, Application |
| Notification | 9 | 2 | Application, Infrastructure |
| SalesDocuments | 12 | 2 | Domain, Application |
| Teams | 12 | 2 | Domain, Application |
| Booking | 12 | 3 | Domain, Application, Client |
| CustomFields | 11 | 3 | Domain, Application, Client |
| Deals | 15 | 3 | Domain, Application, Client |
| Notes | 12 | 3 | Domain, Application, Client |
| Workflow | 12 | 3 | Domain, Application, Client |
| Calendar | 14 | 4 | Domain, Application, Client, Infrastructure |
| FeatureManagement | 14 | 4 | Domain, Application, Client, Grpc |

---

## С чего начать

- **Один вечер.** №9 (два проекта в solution), №14 (правило про события), №4 (ShutdownAsync),
  №5 (nullable-параметры в Customer и Workflow) — механические правки с очевидным результатом.
- **Следующий спринт.** №1 и №3 — они закрывают самые дешёвые способы навредить системе снаружи.
  №1 стоит делать первым: пока он не сделан, любые «расставим права» в модулях будут иллюзией.
- **Планово.** №2 требует решения о модели авторизации (пользователь против сервиса — вопрос уже
  висит в заметках по service-auth), №6 и №7 — переписывания выборок, №11 — новых тестов.
