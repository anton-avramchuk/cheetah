# План реализации архитектуры CRM

Статус текущих модулей:
- ✅ Identity, Features, Crm.Proxy
- ⚠️ Recruitment — содержит чужие домены (Customer, Position, StackItem, WorkFormat)
- ⚠️ Candidates — содержит чужой домен (CandidateApplication, CandidateStage, CandidateSource)
- ⚠️ VacancyTasks — работает, но TaskPriority как entity под вопросом
- ❌ MasterData — не создан
- ❌ Customers — не создан (живёт внутри Recruitment)
- ❌ ApplicantTracking — не создан (CandidateApplication/Stage пока в Candidates)
- ❌ Activities — не создан

---

## Шаг 1 — Создать модуль `MasterData`

**Что переносим из существующих модулей:**
- `Position` + `PositionRepository` — из `Recruitment`
- `StackItem` + `StackItemRepository` — из `Recruitment`
- `WorkFormat` + `WorkFormatRepository` — из `Recruitment`
- `CandidateSource` + `CandidateSourceRepository` — из `Candidates`

**Что добавляем нового:**
- `Skill` (Name, CategoryId) — навыки/технологии с категоризацией
- `SkillCategory` (Name) — категория навыков ("Backend", "Базы данных", "Soft skills")
- `Location` (Country, City, Timezone) — локации для вакансий и кандидатов
- `Industry` (Name) — сфера деятельности компании (FinTech, E-Commerce)

**Изменения в `Position`:**
- Добавить поле `Grade` (enum: Junior, Middle, Senior, Lead, Principal)

**Изменения в существующих модулях после создания MasterData:**
- Recruitment: удалить Position, StackItem, WorkFormat + их команды/запросы/эндпоинты/конфигурации/репозитории
- Candidates: удалить CandidateSource + её команды/запросы/эндпоинты/конфигурации
- Recruitment.Vacancy: поле `PositionId`, `StackItemId`, `WorkFormatId` теперь ссылаются на MasterData (через ID, без FK через EF)
- Candidates: добавить `LocationId` (ссылка на MasterData.Location)

**Модуль:** `Crm.MasterData.*` в `/Modules/MasterData/`
**Схема БД:** `master_data`

---

## Шаг 2 — Создать модуль `Customers`

**Что переносим из `Recruitment`:**
- `Customer` (Name, Code, Description, DirectionId) — переименовать `DirectionId` → `CategoryId`. Добавить `IndustryId` (ссылка на MasterData.Industry без FK).
- `CustomerDirection` — переименовать в `CustomerCategory` (Name, Description)

**Что добавляем нового:**
- `ContactPerson` (CustomerId, FirstName, LastName, Email, Phone, Role, IsDecisionMaker)

**Изменения в `Recruitment` после создания Customers:**
- Удалить Customer, CustomerDirection + всё связанное
- `Vacancy.CustomerId` ссылается на Customers (через ID, без FK)

**Модуль:** `Crm.Customers.*` в `/Modules/Customers/`
**Схема БД:** `customers`

---

## Шаг 3 — Создать модуль `ApplicantTracking`

**Новые агрегаты:**

### `HiringPipeline` (AggregateRoot)
- VacancyId (ссылка на Recruitment без FK)
- Name, IsActive
- Создаётся автоматически при получении `VacancyCreatedEvent` из Recruitment

### `HiringStage` (Entity внутри Pipeline)
- PipelineId, Name, Order, Color
- StageType (enum: New, Screening, Interview, Offer, Hired, Rejected)

### `Application` (AggregateRoot) — перенос из Candidates
- CandidateId (ссылка на Candidates без FK)
- VacancyId (ссылка на Recruitment без FK)
- CurrentStageId, AppliedAt, SourceId, RejectReason, Score
- Метод: `MoveToStage(Guid newStageId)` → публикует `ApplicationStageChangedEvent`

### `Interview` (Entity)
- ApplicationId, ScheduledAt, DurationMinutes, LocationOrLink
- InterviewerIds (JSON/отдельная таблица), Status (Scheduled, Completed, Canceled)
- GeneralFeedback

**Что удаляем из `Candidates`:**
- `CandidateApplication` и всё связанное (команды, запросы, эндпоинты, миграции)
- `CandidateStage` и всё связанное

**Интеграционные события:**
- Слушает: `VacancyCreatedEvent` (из Recruitment) → создаёт HiringPipeline со стартовыми этапами
- Публикует: `ApplicationStageChangedEvent(ApplicationId, CandidateId, VacancyId, OldStageId, NewStageId)`
- Публикует: `ApplicationCreatedEvent(ApplicationId, CandidateId, VacancyId)`

**Локальные данные (Read-Only ACL):**
- Создать локальную сущность `User` (Id, Name, Email) для хранения интервьюеров. Обновляется через события из `Identity`.

**Модуль:** `Crm.ApplicantTracking.*` в `/Modules/ApplicantTracking/`
**Схема БД:** `applicant_tracking`

---

## Шаг 4 — Дополнить модуль `Candidates`

**Новые сущности:**

### `CandidateSkill` (Entity)
- CandidateId, SkillId (ссылка на MasterData без FK)
- Level (enum: Beginner, Junior, Middle, Senior, Expert)

### `CandidateExperience` (Entity)
- CandidateId, CompanyName, Position (строка, не FK)
- StartDate, EndDate (nullable), Description, IsCurrentJob

### `CandidateEducation` (Entity)
- CandidateId, Institution, Specialty, StartYear, EndYear (nullable), Degree

**Дополнения к существующему `Candidate`:**
- Добавить `LocationId` (ссылка на MasterData.Location)
- `ExpectedSalaryMin`, `ExpectedSalaryMax`, `Currency` — зарплатные ожидания

**Локальные данные (Read-Only ACL):**
- Создать `LocalSkill` и `LocalLocation` для быстрых SQL-джойнов при поиске. Обновлять через шину из `MasterData`.

**Упростить `Candidate`:**
- `CandidateExternalProfile` уже есть ✅

**Удалить из Candidates:**
- `CandidateSource` (переехала в MasterData — шаг 1)
- `CandidateApplication`, `CandidateStage` (переехали в ApplicantTracking — шаг 3)

---

## Шаг 5 — Дополнить модуль `Recruitment`

**Новая сущность:**

### `VacancyRequirement` (Entity)
- VacancyId, SkillId (ссылка на MasterData без FK)
- IsMandatory (обязательный/желательный навык)
- YearsOfExperience (требуемый опыт в годах)

**Дополнения к `Vacancy`:**
- `SalaryMin`, `SalaryMax`, `Currency` — зарплатная вилка
- `LocationId` (ссылка на MasterData.Location)
- Убрать `StackItemId` (одна технология) → заменить на `VacancyRequirement` (список навыков)
- `VacancyState` перевести из отдельной таблицы в фиксированный `enum` (Draft, Active, Paused, Closed).

**Новая сущность `VacancyAssignment`:**
- `VacancyId`, `RecruiterId` (пользователь Identity из ACL), `Role` (enum: Lead, Sourcer).

**Локальные данные (Read-Only ACL):**
- Создать `LocalSkill` и `LocalLocation` для быстрого поиска вакансий. Обновлять по шине. (Сущность `User` здесь уже есть ✅)

**После шага 1 и 2:**
- Удалить Position, StackItem, WorkFormat, Customer, CustomerDirection из Recruitment

---

## Шаг 6 — Создать модуль `Activities`

**Агрегат `Activity`:**
- Title, Type (enum: Call, Email, Meeting, Reminder)
- ScheduledAt, CompletedAt (nullable), ResultDescription
- AuthorId (ссылка на Identity без FK)
- **Полиморфный ключ:** TargetEntityType (enum: Candidate, Vacancy, Customer, Application), TargetEntityId (Guid)

**Агрегат `Comment`:**
- Text, AuthorId
- Category (enum: UserNote, CallSummary, SystemLog)
- **Полиморфный ключ:** TargetEntityType, TargetEntityId

**Локальные данные (Read-Only ACL):**
- Создать локальную сущность `User` (Id, Name, Email) для хранения авторов `AuthorId`. Обновляется через события из `Identity`.

**Модуль:** `Crm.Activities.*` в `/Modules/Activities/`
**Схема БД:** `activities`

---

## Шаг 7 — Замена динамических статусов на Enums и добавление ACL

**Проблема:** `TaskPriority`, `TaskState` (в VacancyTasks) и `VacancyState` (в Recruitment) реализованы как кастомизируемые entities, но они сильно завязаны на код/события.

**Решение:** 
1. Перевести `TaskPriority` в `enum` (Low, Normal, High, Critical).
2. Перевести `TaskState` в `enum` (ToDo, InProgress, Review, Done).
3. Перевести `VacancyState` в `enum` (Draft, Active, Closed).
- Удалить их таблицы и CRUD эндпоинты, заменить поля в сущностях на тип `enum`. (Перед удалением написать миграцию).

**Локальные данные (Read-Only ACL) в Tasks:**
- Добавить локальную реплику `User` (из Identity), чтобы хранить имена для `AssignerId` и `AssigneeId`.

---

## Порядок выполнения

| # | Шаг | Зависимости |
|---|-----|-------------|
| 1 | Создать MasterData | — |
| 2 | Создать Customers | — |
| 3 | Очистить Recruitment (удалить Position/StackItem/WorkFormat/Customer) | 1, 2 |
| 4 | Очистить Candidates (удалить CandidateSource) | 1 |
| 5 | Создать ApplicantTracking | 1 |
| 6 | Удалить CandidateApplication/Stage из Candidates | 5 |
| 7 | Дополнить Candidates (Skill/Experience/Education) | 1 |
| 8 | Дополнить Recruitment (VacancyRequirement, зарплата) | 1 |
| 9 | Создать Activities | — |
| 10 | Исправить TaskPriority → enum | — |

---

## Правила при реализации

- Никаких FK между модулями в EF Core — только `Id` как `Guid`
- Денормализация допустима (хранить имя кандидата в Application для скорости)
- При удалении сущности из модуля — писать Down-миграцию
- События публиковать ПОСЛЕ `SaveChangesAsync()`
- Каждый новый модуль — полная структура по CLAUDE.md (8 проектов)
