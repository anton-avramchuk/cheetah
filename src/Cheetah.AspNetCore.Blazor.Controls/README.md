# Cheetah.AspNetCore.Blazor.Controls

Библиотека Blazor-компонентов на базе Bootstrap 5 для Cheetah Blazor-хоста (BFF), включая `Kanban` (drag-and-drop доска).
Все компоненты поддерживают акцентный цвет приложения (`--accent-primary`) и работают вместе с дизайн-токенами темы (`Cheetah.AspNetCore.Blazor.Theme`).

## Подключение

Добавьте `using` в `_Imports.razor` нужного проекта:

```razor
@using Cheetah.AspNetCore.Blazor.Controls
```

Подключите CSS-бандл в `<head>` хоста:

```html
<link rel="stylesheet" href="_content/Cheetah.AspNetCore.Blazor.Controls/Cheetah.AspNetCore.Blazor.Controls.styles.css" />
```

> Компоненты используют CSS-изоляцию (`.razor.css`), поэтому в Blazor-хосте достаточно подключить общий бандл `{Assembly}.styles.css`.

---

## Компоненты

### CrmTextBox

Текстовое поле ввода.

```razor
<CrmTextBox Label="Имя"
            Placeholder="Введите имя"
            @bind-Value="model.Name"
            Required
            Error="@errors["Name"]"
            Hint="Не более 100 символов"
            MaxLength="100" />
```

| Параметр | Тип | По умолч. | Описание |
|---|---|---|---|
| `Value` | `string?` | — | Значение (поддерживает `@bind`) |
| `Label` | `string` | `""` | Подпись |
| `Placeholder` | `string` | `""` | Плейсхолдер |
| `Error` | `string?` | — | Текст ошибки (показывает `is-invalid`) |
| `Hint` | `string` | `""` | Подсказка под полем |
| `Required` | `bool` | `false` | Добавляет `*` к label |
| `Disabled` | `bool` | `false` | — |
| `ReadOnly` | `bool` | `false` | — |
| `MaxLength` | `int` | `0` | 0 — без ограничений |
| `OnBlur` | `EventCallback<string?>` | — | Срабатывает при потере фокуса |

---

### CrmTextArea

Многострочное текстовое поле.

```razor
<CrmTextArea Label="Описание"
             Rows="5"
             @bind-Value="model.Description" />
```

Параметры совпадают с `CrmTextBox`, плюс `Rows` (по умолчанию `3`).

---

### CrmNumberBox\<T\>

Числовой ввод. `T` — любой числовой тип (`int`, `decimal`, `double` и т.д.).

```razor
<CrmNumberBox TValue="int"
              Label="Зарплата"
              Min="0"
              Max="1000000"
              Step="1000"
              @bind-Value="model.Salary" />
```

| Параметр | Тип | Описание |
|---|---|---|
| `Min` / `Max` | `string?` | Строковые значения атрибутов `min`/`max` |
| `Step` | `string` | Шаг (по умолч. `"1"`) |

---

### CrmPasswordBox

Поле пароля с кнопкой показать/скрыть.

```razor
<CrmPasswordBox Label="Пароль"
                AutoComplete="new-password"
                @bind-Value="model.Password"
                Error="@errors["Password"]" />
```

---

### CrmSearchBox

Строка поиска с иконкой, кнопкой очистки и событием по Enter.

```razor
<CrmSearchBox Placeholder="Поиск кандидатов…"
              @bind-Value="_query"
              OnSearch="SearchAsync" />
```

| Параметр | Тип | Описание |
|---|---|---|
| `OnSearch` | `EventCallback<string?>` | Срабатывает по Enter и при очистке |

---

### CrmSelect\<TValue\>

Выпадающий список с обобщённым типом элементов. Кастомный дропдаун (не нативный `<select>`),
поэтому элементы могут содержать произвольную разметку через `ItemTemplate` — например флаг + имя.
Закрывается по клику вне списка и по `Escape`; открывается с клавиатуры (`Enter`/`Space`/`↓`).

```razor
<!-- Простой вариант: текст через LabelSelector -->
<CrmSelect TValue="SkillCategory"
           Label="Категория"
           Items="@categories"
           LabelSelector="c => c.Name"
           Placeholder="— выберите —"
           @bind-Value="model.Category" />

<!-- С шаблоном элемента: флаг + название -->
<CrmSelect TValue="Country"
           Label="Страна"
           Items="@countries"
           LabelSelector="c => c.Name"
           Placeholder="— выберите —"
           @bind-Value="model.Country">
    <ItemTemplate Context="c">
        <img src="@c.FlagUrl" width="20" alt="" />
        <span>@c.Name</span>
    </ItemTemplate>
</CrmSelect>
```

| Параметр | Тип | Описание |
|---|---|---|
| `Items` | `IEnumerable<TValue>` | Коллекция элементов |
| `KeySelector` | `Func<TValue, string>` | Ключ элемента (для сравнения значений) |
| `LabelSelector` | `Func<TValue, string>` | Текст элемента (fallback, если нет `ItemTemplate`) |
| `Placeholder` | `string` | Текст, когда ничего не выбрано |
| `ItemTemplate` | `RenderFragment<TValue>` | Шаблон элемента списка; без него выводится `LabelSelector` |
| `SelectedTemplate` | `RenderFragment<TValue>` | Шаблон выбранного значения в свёрнутом виде; без него — `ItemTemplate`, затем `LabelSelector` |

---

### CrmRadioGroup\<TValue\>

Группа радиокнопок.

```razor
<CrmRadioGroup TValue="Grade"
               Label="Грейд"
               Items="Enum.GetValues<Grade>()"
               LabelSelector="g => g.ToString()"
               Inline
               @bind-Value="model.Grade" />
```

| Параметр | Тип | Описание |
|---|---|---|
| `Inline` | `bool` | Расположить кнопки в строку |

---

### CrmCheckBox

Чекбокс с label и поддержкой валидации.

```razor
<CrmCheckBox Label="Согласен с условиями"
             @bind-Value="model.Agreed"
             Error="@errors["Agreed"]" />
```

---

### CrmSwitch

Bootstrap toggle-переключатель.

```razor
<CrmSwitch Label="Активен"
           @bind-Value="model.IsActive"
           Hint="Снимите флаг, чтобы скрыть запись" />
```

---

### CrmDatePicker

Выбор даты (`DateOnly`).

```razor
<CrmDatePicker Label="Дата рождения"
               Min="1950-01-01"
               Max="2010-12-31"
               @bind-Value="model.BirthDate" />
```

---

### CrmDateTimePicker

Выбор даты и времени (`DateTime`).

```razor
<CrmDateTimePicker Label="Дата интервью"
                   @bind-Value="model.InterviewAt" />
```

---

### CrmFileUpload

Загрузка файлов через `InputFile`.

```razor
<CrmFileUpload Label="Резюме"
               Accept=".pdf,.doc,.docx"
               Hint="Максимум 5 МБ"
               OnFileSelected="HandleFile" />

<!-- Множественный выбор -->
<CrmFileUpload Multiple
               OnFilesSelected="HandleFiles" />
```

| Параметр | Тип | Описание |
|---|---|---|
| `Accept` | `string` | MIME или расширения |
| `Multiple` | `bool` | Разрешить несколько файлов |
| `OnFileSelected` | `EventCallback<IBrowserFile>` | Одиночный файл |
| `OnFilesSelected` | `EventCallback<IReadOnlyList<IBrowserFile>>` | Несколько файлов |

---

### CrmButton

Кнопка со спиннером, иконкой и вариантами Bootstrap.

```razor
<CrmButton Variant="primary" Loading="_saving" OnClick="SaveAsync">
    Сохранить
</CrmButton>

<CrmButton Variant="danger" Outline Size="sm" OnClick="DeleteAsync">
    <IconContent><!-- SVG иконка --></IconContent>
    Удалить
</CrmButton>
```

| Параметр | Тип | Описание |
|---|---|---|
| `Variant` | `string` | `primary`, `secondary`, `danger`, `success` и т.д. |
| `Size` | `string` | `sm`, `md`, `lg` |
| `Loading` | `bool` | Показывает спиннер, блокирует клик |
| `Outline` | `bool` | Использует `btn-outline-*` |
| `Type` | `string` | `button` / `submit` / `reset` |

---

### CrmBadge

Бейдж-метка.

```razor
<CrmBadge Variant="success" Pill>Активен</CrmBadge>
<CrmBadge Variant="warning">На рассмотрении</CrmBadge>
```

---

### CrmAlert

Информационный блок с опциональным закрытием.

```razor
<CrmAlert Variant="warning"
          Title="Внимание"
          Message="Данные не сохранены."
          Dismissible
          OnDismissed="OnAlertClosed" />

<!-- Или произвольный контент -->
<CrmAlert Variant="info" Dismissible>
    Кандидат успешно добавлен. <a href="/candidates">Перейти</a>
</CrmAlert>
```

---

### CrmSpinner

Индикатор загрузки, опционально с оверлеем.

```razor
<CrmSpinner Visible="_loading" Text="Загрузка…" />

<!-- Полноэкранный оверлей -->
<CrmSpinner Visible="_saving" Overlay />

<!-- Grow-вариант -->
<CrmSpinner Grow Variant="secondary" />
```

---

### CrmProgressBar

Прогресс-бар с поддержкой анимации.

```razor
<CrmProgressBar Value="75"
                Label="Заполненность профиля"
                Variant="success"
                ShowValue
                Striped
                Animated />
```

---

### CrmCard

Карточка с именованными слотами.

```razor
<CrmCard Title="Кандидат">
    <p>Иван Иванов</p>
</CrmCard>

<!-- С header и footer -->
<CrmCard>
    <HeaderContent>
        <div class="d-flex justify-content-between">
            <strong>Вакансия</strong>
            <CrmBadge Variant="success">Открыта</CrmBadge>
        </div>
    </HeaderContent>
    <ChildContent>…</ChildContent>
    <FooterContent>
        <CrmButton Size="sm">Подробнее</CrmButton>
    </FooterContent>
</CrmCard>
```

---

### CrmTabs + CrmTabPanel

Вкладки. По умолчанию `CrmTabPanel` рендерит контент только при активной вкладке и
уничтожает его при переключении (контент пересоздаётся при каждом возврате).

```razor
<CrmTabs OnTabChanged="OnTabChanged">
    <CrmTabPanel Key="info" Title="Основное">
        …
    </CrmTabPanel>
    <CrmTabPanel Key="skills" Title="Навыки">
        …
    </CrmTabPanel>
    <CrmTabPanel Key="history" Title="История" Disabled>
        …
    </CrmTabPanel>
</CrmTabs>

<!-- Pills-вариант -->
<CrmTabs Pills>…</CrmTabs>
```

**Отложенная инициализация (`Lazy`).** В ленивом режиме контент вкладки создаётся
только при первом открытии и затем сохраняется (скрывается через CSS), то есть
инициализируется один раз — состояние не теряется и `OnInitialized` контента не
вызывается повторно при переключениях. Удобно для тяжёлых вкладок (загрузка данных,
дорогие компоненты), которые не нужно поднимать заранее.

Режим можно задать сразу для всех панелей на `CrmTabs` либо точечно на конкретной
`CrmTabPanel` (переопределяет значение контейнера):

```razor
<!-- Лениво инициализируются все вкладки -->
<CrmTabs Lazy>
    <CrmTabPanel Key="info"    Title="Основное">…</CrmTabPanel>
    <CrmTabPanel Key="reports" Title="Отчёты">…</CrmTabPanel>   <!-- создастся при первом открытии -->
    <!-- Переопределение для отдельной панели: всегда рендерить только когда активна -->
    <CrmTabPanel Key="live" Title="Онлайн" Lazy="false">…</CrmTabPanel>
</CrmTabs>
```

---

### CrmAccordion + CrmAccordionItem

Аккордеон.

```razor
<CrmAccordion>
    <CrmAccordionItem Title="Опыт работы" DefaultOpen>
        …
    </CrmAccordionItem>
    <CrmAccordionItem Title="Образование">
        …
    </CrmAccordionItem>
</CrmAccordion>
```

---

### CrmTooltip

Обёртка для Bootstrap tooltip (требует инициализации `bootstrap.js`).

```razor
<CrmTooltip Text="Нельзя удалить активную запись" Placement="bottom">
    <CrmButton Variant="danger" Disabled>Удалить</CrmButton>
</CrmTooltip>
```

---

## Общие параметры

Большинство компонентов поддерживают:

| Параметр | Тип | Описание |
|---|---|---|
| `Label` | `string` | Подпись над полем |
| `Error` | `string?` | Текст ошибки (активирует класс `is-invalid`) |
| `Hint` | `string` | Серая подсказка под полем (скрывается при Error) |
| `Class` | `string` | Дополнительные CSS-классы на корневой элемент |
| `Disabled` | `bool` | Блокирует взаимодействие |
| `Required` | `bool` | Добавляет `*` к label |

## Зависимости

- `Cheetah.Core` + `Cheetah.AspNetCore.Blazor.Dialogs` (`CrmBlazorControlsModule` → `[DependsOn(CoreModule, CrmBlazorDialogsModule)]`).
- `Microsoft.AspNetCore.App` (Razor-компоненты, `InputFile`, JS-interop).
- Модуль не содержит `[Export]`-сервисов — это библиотека компонентов; для использования достаточно `@using` и `[DependsOn(typeof(CrmBlazorControlsModule))]`.
- `CrmTooltip` требует инициализированного `bootstrap.js` на стороне хоста.
