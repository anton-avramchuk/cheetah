# Kanban Board Implementation Plan

## Context

Two kanban boards: vacancies (by VacancyState) and tasks (by TaskState). Cards open as Jira-style slide-over panel for quick editing + option to navigate to full page. Customer gets Code (prefix like Jira: SBER-1, SBER-2). DB seeded with default data.

## Current State

**Already exists:**
- Kanban components: `CrmKanbanBoard<T>`, `CrmKanbanColumn<T>`, `CrmKanbanCard<T>` — ready, HTML5 DnD, CSS
- `VacancyTask.Move(stateId, order)` + `MoveVacancyTaskEndpoint` (PATCH) — works
- `TaskState` with `Color` and `IsDefault` — full model
- `RecruitmentDbContextSeeder` — seeds VacancyState (5 statuses) and VacancyRole (3 roles)
- Endpoint pattern: `PatchCommandEndpoint<TRequest, TCommand>` (see `MoveVacancyTaskEndpoint.cs`)

**Missing:**
- VacancyState lacks `Color`/`IsDefault` — needs migration
- Customer lacks `Code` — needs migration
- VacancyTask lacks `Number` (seq ID) — needs migration
- Vacancy lacks `Order` and `Move()` — needs domain changes
- No API for VacancyState (no CRUD, no ViewModel, no Contract)
- No seeder for VacancyTasks (TaskState, TaskPriority empty)
- No SlideOver component
- No Kanban pages

---

## Phase 1: Domain + Migrations ✅ DONE

### 1.1 VacancyState — add Color + IsDefault ✅

**File:** `src/Modules/Recruitment/Crm.Recruitment.Domain/VacancyState.cs`
- Add `Color? Color` and `bool IsDefault` (same pattern as `TaskState.cs`)
- Update `Create(name, order, color?, isDefault)` and `Update(name, order, color?, isDefault)`

**File:** `src/Modules/Recruitment/Crm.Recruitment.DataAccess/Configurations/VacancyStateConfiguration.cs`
- `builder.Property(x => x.Color).HasMaxLength(9)` with Color<->string converter
- `builder.Property(x => x.IsDefault).IsRequired()`

### 1.2 Customer — add Code ✅

**File:** `src/Modules/Recruitment/Crm.Recruitment.Domain/Customer.cs`
- Add `string? Code` (uppercase, max 20)
- Update `Create(name, description?, directionId?, code?)` + method `SetCode(code?)`

**File:** Customer configuration
- `builder.Property(x => x.Code).HasMaxLength(20)`
- `builder.HasIndex(x => x.Code).IsUnique().HasFilter(...)` — filtered unique index

### 1.3 Vacancy — add Order + Move() ✅

**File:** `src/Modules/Recruitment/Crm.Recruitment.Domain/Vacancy.cs`
- Add `int Order`
- Add method `Move(Guid? stateId, int order)` { StateId = stateId; Order = order; }

**File:** Vacancy configuration
- `builder.Property(x => x.Order).IsRequired().HasDefaultValue(0)`

### 1.4 VacancyTask — add Number ✅

**File:** `src/Modules/VacancyTasks/VacancyTasks/Crm.VacancyTasks.Domain/VacancyTask.cs`
- Add `int Number`
- Update `Create(...)` — add parameter `number`

**File:** VacancyTask configuration
- `builder.Property(x => x.Number).IsRequired()`
- `builder.HasIndex(x => new { x.VacancyId, x.Number }).IsUnique()`

### 1.5 Migrations (pending — apply after review)

```
dotnet ef migrations add AddColorIsDefaultToVacancyState -p src/.../Crm.Recruitment.DataAccess
dotnet ef migrations add AddVacancyTaskNumber -p src/.../Crm.VacancyTasks.DataAccess
```

---

## Phase 2: Seed Data ✅ DONE

### 2.1 Update RecruitmentDbContextSeeder ✅

**File:** `src/Modules/Recruitment/Crm.Recruitment.DataAccess/Services/RecruitmentDbContextSeeder.cs`

VacancyStates with colors:
| Name | Order | Color | IsDefault |
|------|-------|-------|-----------|
| To do | 0 | #6c757d | true |
| In Progress | 1 | #0d6efd | false |
| Pause | 2 | #ffc107 | false |
| Completed | 3 | #198754 | false |
| Cancelled | 4 | #dc3545 | false |

Add `SeedCustomersAsync` with demo data:
- "Sberbank" code=SBER, "VTB Bank" code=VTB, "Yandex" code=YND

### 2.2 Create VacancyTasksDbContextSeeder ✅

**New file:** `src/Modules/VacancyTasks/VacancyTasks/Crm.VacancyTasks.DataAccess/Services/VacancyTasksDbContextSeeder.cs`

TaskStates:
| Name | Order | Color | IsDefault |
|------|-------|-------|-----------|
| To do | 0 | #6c757d | true |
| In Progress | 1 | #0d6efd | false |
| In Review | 2 | #fd7e14 | false |
| Done | 3 | #198754 | false |
| Cancelled | 4 | #dc3545 | false |

TaskPriorities:
| Name | Order | Color |
|------|-------|-------|
| Critical | 0 | #dc3545 |
| High | 1 | #fd7e14 |
| Medium | 2 | #ffc107 |
| Low | 3 | #198754 |

Pattern: `[Export(LifetimeType.Scoped, typeof(IDatabaseSeeder))]`, idempotent via `AnyAsync()`.

---

## Phase 3: API — Move Vacancy + VacancyState CRUD + Board Queries ✅ DONE

### 3.1 VacancyState API (full CRUD — currently no endpoints) ✅

Create full stack:
- **Application:** `VacancyStateModel`, `GetAllVacancyStatesQuery` + handler, `GetVacancyStateByIdQuery` + handler, `CreateVacancyStateCommand` + handler, `UpdateVacancyStateCommand` + handler, `DeleteVacancyStateCommand` + handler
- **Contracts:** `VacancyStateViewModel(Id, Name, Order, Color?, IsDefault)`, requests with `[ApiRoute]`
- **Api:** 5 endpoints (Create/Update/Delete/GetById/GetAll) following existing patterns (TaskState in VacancyTasks as reference)

### 3.2 MoveVacancy endpoint ✅

- **Command:** `MoveVacancyCommand(Guid Id, Guid StateId, int Order) : ICommand`
- **Handler:** load Vacancy -> `entity.Move(stateId, order)` -> SaveChanges
- **Contract:** `MoveVacancyRequest` with `[ApiRoute("api/vacancies/{id:guid}/move", ApiMethod.Patch)]`
- **Endpoint:** `MoveVacancyEndpoint : PatchCommandEndpoint<MoveVacancyRequest, MoveVacancyCommand>`

Reference: `MoveVacancyTaskEndpoint.cs`

### 3.3 Extend VacancyViewModel/VacancyModel ✅

Add to `VacancyModel` and `VacancyViewModel`:
- `string? CustomerCode` — for card prefix display
- `string? StateColor` — for column color
- `int Order` — for sorting within column

Update `GetVacancyByIdQueryHandler` and list query — pull `Customer.Code`, `State.Color`.

### 3.4 Extend VacancyTaskViewModel ✅

Add:
- `int Number` — sequential task number
- `string? StateName` — state name
- `string? PriorityName`, `string? PriorityColor` — for priority badge

Update `CreateVacancyTaskCommandHandler`:
```csharp
var maxNumber = await _repository.AsNoTrackingQueryable()
    .Where(t => t.VacancyId == command.VacancyId)
    .MaxAsync(t => (int?)t.Number, ct) ?? 0;
var task = VacancyTask.Create(..., number: maxNumber + 1);
```

### 3.5 Update Customer contracts ✅

Add `Code` to `CustomerViewModel`, `CreateCustomerRequest`, `UpdateCustomerRequest`.
Update `CreateCustomerCommand`/`UpdateCustomerCommand` and their handlers.

---

## Phase 4: CrmSlideOver Component ✅ DONE

**New file:** `src/Cheetah.Blazor.Components/SlideOver/CrmSlideOver.razor` ✅

Bootstrap offcanvas panel on the right. No JS — pure CSS transition.

Parameters:
- `bool IsVisible` + `EventCallback<bool> IsVisibleChanged` (two-way binding)
- `string? Title`, `string? Subtitle`
- `RenderFragment? Header`, `RenderFragment? ChildContent`, `RenderFragment? Footer`
- `string Width = "560px"`
- `string? ExternalUrl` — "Open full page" button (-> separate page)
- `EventCallback OnClose`
- `bool CloseOnBackdropClick = true`

Public methods: `ShowAsync()`, `CloseAsync()`

**New file:** `src/Cheetah.Blazor.Components/wwwroot/css/slideover.css` ✅
- `.crm-slideover` with `transform: translateX(100%)` -> `.show` = `translateX(0)`
- transition 0.3s, backdrop fade

Add `@using Cheetah.Blazor.Components.SlideOver` to `_Imports.razor` ✅

---

## Phase 5: Kanban Pages ✅ DONE

### 5.1 Vacancy Board ✅

**New file:** `src/Modules/Recruitment/Crm.Recruitment.Frontend/Pages/VacancyBoardPage.razor`

Route: `@page "/vacancies/board"`

1. OnInitializedAsync: load all VacancyState (columns) ordered by Order, load all Vacancy grouped by StateId sorted by Order
2. `CrmKanbanBoard<VacancyViewModel>`: for each state -> CrmKanbanColumn with Id=state.Id, Title=state.Name; HeaderTemplate with `style="border-top: 3px solid {state.Color}"`; CardTemplate: Name, Customer badge (code + name), Position badge
3. OnItemMoved -> `VacanciesService.PatchAsync(id, MoveVacancyRequest)` + optimistic local Dictionary update
4. OnCardClick -> open CrmSlideOver with VacancySlideOverForm

### 5.2 VacancySlideOverForm ✅

**New file:** `src/Modules/Recruitment/Crm.Recruitment.Frontend/Components/VacancySlideOverForm.razor`

Inline form for slide-over:
- Load vacancy by Id via `VacanciesService.GetByIdAsync`
- Dictionaries from `ICacheService` (already implemented)
- Fields: Name, Description, State (select), Customer, Position, Stack, WorkFormat
- Save -> `VacanciesService.UpdateAsync` -> `EventCallback OnSaved`
- "Open full page" button via ExternalUrl -> `/vacancies/{id}/edit`

### 5.3 VacancyTask Board ✅

**New file:** `src/Modules/VacancyTasks/VacancyTasks/Crm.VacancyTasks.Frontend/Pages/VacancyTaskBoardPage.razor`

Route: `@page "/vacancy-tasks/board"`

1. Load TaskState (columns) + all VacancyTask
2. Group by StateId
3. CardTemplate: "#" + Number, Title, Priority badge (colored), DueDate (red if overdue)
4. OnItemMoved -> `VacancyTasksService.PatchAsync(id, MoveVacancyTaskRequest)` (endpoint already exists!)
5. OnCardClick -> CrmSlideOver with VacancyTaskSlideOverForm

### 5.4 VacancyTaskSlideOverForm ✅

**New file:** `src/Modules/VacancyTasks/VacancyTasks/Crm.VacancyTasks.Frontend/Components/VacancyTaskSlideOverForm.razor`

Fields: Title, Description, State (select), Priority (select with colors), DueDate.
ExternalUrl -> `/vacancy-tasks/{id}/edit`

### 5.5 Menu ✅

**File:** `src/Modules/Recruitment/Crm.Recruitment.Frontend/Navigation/RecruitmentMenuContributor.cs`
- Changed "Vacancies" url to `"vacancies/board"` (board as main page)

**File:** `src/Modules/VacancyTasks/VacancyTasks/Crm.VacancyTasks.Frontend/Navigation/VacancyTasksMenuContributor.cs`
- Changed "Tasks" url to `"vacancy-tasks/board"`

---

## File Summary

### Phase 1 — Domain + Migrations (~8 files + 2 migrations)
### Phase 2 — Seed Data (~2 files)
### Phase 3 — API (~25+ files)
### Phase 4 — CrmSlideOver (~2 new files + 1 edit)
### Phase 5 — Frontend Pages (~6 files)
