# Cheetah CRM

Модульный CRM-фреймворк на **.NET 10** с событийно-ориентированной архитектурой. Монолит с подготовкой к микросервисам: каждый модуль может работать как часть единого приложения или быть вынесен в отдельный сервис без переписывания кода.

**Frontend:** Angular (standalone SPA), общается с бэкендом по REST API.
**Цель по нагрузке:** 10 000+ RPS.

---

## 🎯 Ключевые идеи

- **Модульность.** Независимые модули общаются через события (Redis/Kafka), у каждого — своя база PostgreSQL. Авторегистрация сервисов через Source Generators, топологическая сортировка зависимостей.
- **DDD + CQRS.** Чистые слои: Events → Shared → Contracts → Domain → Application → Infrastructure → Api (+ опциональный Client).
- **Repository + Specification.** Application-слой не работает с `DbContext` напрямую; вся фильтрация — через спецификации.
- **Только Minimal API.** Контроллеры запрещены, эндпоинты регистрируются в `OnApplicationInitialization` (или декларативно через генератор).
- **Транспорт-агностичность.** REST и gRPC сосуществуют через реестр транспортов; несколько провайдеров для данных, кэша, шины событий, файлового хранилища.

Подробное руководство для разработчиков — в [`CLAUDE.md`](CLAUDE.md).

---

## 🏗️ Структура репозитория

```
src/
├── Cheetah.Core.*            # Ядро: модульность, CQRS, домен, события, спецификации
├── Cheetah.Backend.*         # Инфраструктура: события (Redis/Kafka), JWT/RSA, gRPC, ReverseProxy, ServiceAuth/UserAuth
├── Cheetah.Core.EntityFramework.*  # EF Core + провайдеры (PostgreSql/MsSql/MySql/Sqlite)
├── Cheetah.Core.Dapper.*     # Dapper DAL + провайдеры
├── Cheetah.Core.Mongo        # MongoDB DAL (Outbox/Inbox/Audit/Saga)
├── Cheetah.AspNetCore.*      # ASP.NET Core хост + (legacy) Blazor-компоненты
├── Cheetah.Mapping.*         # Маппинг: Mapster + генератор маппинга
├── Cheetah.Generators.*      # Source Generators (модули, эндпоинты, API-клиенты, gRPC)
├── Cheetah.Audit / Saga / Outbox / Inbox / FileStorage / Notifications / ...  # Кросс-модульные возможности
└── Modules/                  # Бизнес-модули
```

### Бизнес-модули (`src/Modules/`)

| Модуль | Назначение |
|---|---|
| **Identity** | Пользователи, JWT-аутентификация |
| **Permissions** | RBAC |
| **Customer** | Клиенты (расширяемый шаблон-агрегат) |
| **Deals** | Сделки/воронка (StateMachine, Outbox) |
| **Activities** | Задачи/активности |
| **Leads** | Лиды + конвертация через порт-оркестратор |
| **Catalog** | Товары/категории/прайс-листы |
| **SalesDocuments** | КП/заказы/счета (один агрегат + StateMachine) |
| **Calendar** | События с RRULE, напоминания, free/busy |
| **Booking** | Запись на слоты (Calendly-стиль) |
| **NotesTimeline** | Заметки + лента хронологии |
| **Tags** | Метки |
| **Teams** | Команды/роли/участники |
| **CustomFields** | Кастомные поля без миграций (JSONB) |
| **FeatureManagement** | Фич-флаги |
| **Workflow** | No-code автоматизация (триггер → условие → действия) |
| **Notification / Email** | Уведомления и email-канал |

Документация по модулям — в [`docs/modules/`](docs/modules/), планы — в [`docs/plans.md`](docs/plans.md).

---

## 🚀 Быстрый старт

### Требования

- [.NET SDK 10.0.100+](https://dotnet.microsoft.com/) (см. [`global.json`](global.json))
- Docker / Docker Compose
- PostgreSQL 17 и Redis 7 (поднимаются через compose)

### Запуск через Docker Compose

```bash
# 1. Подготовьте переменные окружения
cp .env.example .env
# отредактируйте .env: JWT_SECRET_KEY должен быть не короче 32 символов

# 2. Поднимите инфраструктуру и сервисы
docker compose up --build
```

После старта API доступно через reverse-proxy на `http://localhost:5555`.

### Локальная сборка

```bash
dotnet restore
dotnet build Cheetah.slnx
dotnet test Cheetah.slnx
```

> Решение использует XML-формат `.slnx`. Проекты добавляются командой `dotnet sln add <path>`.

---

## 📦 Управление пакетами

- **Central Package Management** через [`Directory.Packages.props`](Directory.Packages.props) в корне.
- В `.csproj` пишите `<PackageReference Include="..." />` **без** атрибута `Version`.
- Версия фреймворка — в [`version.props`](version.props).

### Публикация NuGet

Пакеты `Cheetah.*` публикуются в **GitHub Packages** (workflow [`publish-nuget.yml`](.github/workflows/publish-nuget.yml) срабатывает на тег `v*`). Потребителю требуется токен с правом `read:packages`.

Локальная упаковка: [`pack.ps1`](pack.ps1).

---

## 🧩 Создание нового модуля

Порядок проектов: **Events → Shared → Contracts → Domain → Infrastructure → Application → Api → (Client) → Tests**.
Подробный чек-лист и правила слоёв — в [`CLAUDE.md`](CLAUDE.md). Шаблоны: [`install-templates.ps1`](install-templates.ps1).

---

## 📄 Лицензия

[MIT](LICENSE) © 2025 anton-avramchuk
