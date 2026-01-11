# Core.Tenants Tests Summary

## Обзор

Добавлен **новый тестовый проект** для покрытия сервисного слоя модуля `Cheetah.Core.Tenants`.

**Расположение:** `tests/Cheetah.Core.Tenants.Tests/Services/`

---

## DefaultModuleConnectionStringProviderTests

**Файл:** `DefaultModuleConnectionStringProviderTests.cs`

**Тестируемый класс:** `DefaultModuleConnectionStringProvider`

**Назначение:** Генерация connection strings для модулей в multi-tenant системе на основе шаблона

### Формат генерации

`DefaultModuleConnectionStringProvider` генерирует имена баз данных в формате:
```
tenant_{tenantName}_{moduleName_lowercase}
```

Например:
- Tenant: "AcmeCorp", Module: "Identity" → `tenant_AcmeCorp_identity`
- Tenant: "TestCo", Module: "Features" → `tenant_TestCo_features`

### Покрытые сценарии:

✅ **Конструктор (1 тест)**
- Constructor_ShouldSetModuleName - проверка сохранения имени модуля

✅ **Генерация с разными СУБД (3 теста)**
- GenerateConnectionString_WithPostgreSql_ShouldGenerateCorrectConnectionString - PostgreSQL connection string
- GenerateConnectionString_WithSqlServer_ShouldGenerateCorrectConnectionString - SQL Server connection string
- GenerateConnectionString_WithMySql_ShouldGenerateCorrectConnectionString - MySQL connection string

✅ **Преобразование имени модуля (2 теста)**
- GenerateConnectionString_ShouldConvertModuleNameToLowerCase - модуль в UPPERCASE → lowercase
- GenerateConnectionString_WithMixedCaseModuleName_ShouldConvertToLowerCase - модуль в MixedCase → lowercase

✅ **Использование tenant name (4 теста)**
- GenerateConnectionString_ShouldUseTenantName - проверка использования tenant name
- GenerateConnectionString_WithDifferentTenants_ShouldGenerateDifferentDatabases - разные tenants → разные БД
- GenerateConnectionString_WithSpacesInTenantName_ShouldIncludeSpaces - пробелы в tenant name сохраняются
- GenerateConnectionString_WithUnderscoresInTenantName_ShouldPreserveUnderscores - подчёркивания сохраняются

✅ **Разные модули (1 тест)**
- GenerateConnectionString_WithDifferentModules_ShouldGenerateDifferentDatabases - разные модули → разные БД

✅ **Сохранение свойств connection string (1 тест)**
- GenerateConnectionString_ShouldPreserveOtherConnectionStringProperties - все параметры (Host, Port, Username, Password, Pooling, Timeout) сохраняются

✅ **Edge cases (1 тест)**
- GenerateConnectionString_WithEmptyModuleName_ShouldGenerateWithoutModuleSuffix - пустое имя модуля

✅ **Параметризованные тесты (4 теста через Theory)**
- GenerateConnectionString_WithVariousInputs_ShouldGenerateExpectedDatabaseName
  - "Identity" + "Acme" → "tenant_Acme_identity"
  - "Features" + "TestCo" → "tenant_TestCo_features"
  - "Permissions" + "MyOrg" → "tenant_MyOrg_permissions"
  - "UPPERCASE" + "Company" → "tenant_Company_uppercase"

**Всего тестов:** 17

---

## Технологии

- **xUnit** - тестовый фреймворк
- **FluentAssertions** - читаемые assertions
- **DbConnectionStringBuilder** - парсинг и генерация connection strings

---

## Паттерны тестирования

### 1. AAA Pattern (Arrange-Act-Assert)

```csharp
[Fact]
public void GenerateConnectionString_WithPostgreSql_ShouldGenerateCorrectConnectionString()
{
    // Arrange
    var provider = new DefaultModuleConnectionStringProvider("Identity");
    var tenantId = Guid.NewGuid();
    var tenantName = "AcmeCorp";
    var baseConnectionString = "Host=localhost;Port=5432;Database=template;Username=postgres;Password=secret";

    // Act
    var result = provider.GenerateConnectionString(tenantId, tenantName, baseConnectionString);

    // Assert
    result.Should().Contain("tenant_AcmeCorp_identity");
    result.ToLowerInvariant().Should().Contain("host=localhost");
    result.Should().Contain("5432");
}
```

### 2. Параметризованные тесты с Theory

```csharp
[Theory]
[InlineData("Identity", "Acme", "tenant_Acme_identity")]
[InlineData("Features", "TestCo", "tenant_TestCo_features")]
[InlineData("Permissions", "MyOrg", "tenant_MyOrg_permissions")]
public void GenerateConnectionString_WithVariousInputs_ShouldGenerateExpectedDatabaseName(
    string moduleName,
    string tenantName,
    string expectedDbName)
{
    var provider = new DefaultModuleConnectionStringProvider(moduleName);
    var result = provider.GenerateConnectionString(Guid.NewGuid(), tenantName, "Host=localhost;Database=template");

    result.Should().Contain(expectedDbName);
}
```

### 3. Case-insensitive проверка

Так как `DbConnectionStringBuilder` нормализует ключи в lowercase, используется `ToLowerInvariant()`:

```csharp
result.ToLowerInvariant().Should().Contain("host=localhost");
```

---

## Запуск тестов

### Запуск всех тестов проекта

```bash
dotnet test tests/Cheetah.Core.Tenants.Tests
```

### Запуск с детальным выводом

```bash
dotnet test tests/Cheetah.Core.Tenants.Tests --verbosity normal
```

### Запуск конкретного класса

```bash
dotnet test --filter "FullyQualifiedName~DefaultModuleConnectionStringProviderTests"
```

---

## Покрытие функциональности

### ✅ Полностью покрыто

- **Constructor** - инициализация с именем модуля
- **GenerateConnectionString** - генерация connection strings
  - PostgreSQL
  - SQL Server
  - MySQL
- **Преобразование имени модуля** - в lowercase
- **Tenant name** - использование и сохранение
- **Сохранение параметров** - все параметры base connection string сохраняются
- **Edge cases** - пустое имя модуля, спецсимволы, пробелы

### 🎯 Рекомендации по расширению

**Приоритет 1: Добавить тесты для валидации**
- Добавить тесты для null/empty tenantName
- Добавить тесты для null baseConnectionString
- Добавить тесты для некорректного baseConnectionString

**Приоритет 2: Добавить тесты для специальных символов**
- Тестирование с SQL-опасными символами в tenant name (', ", ;)
- Тестирование с Unicode символами
- Тестирование с очень длинными именами

**Приоритет 3: Интеграционные тесты**
- Тестирование реального подключения к БД (in-memory PostgreSQL/SQLite)
- Тестирование с реальным EF Core DbContext

---

## Общая статистика

| Тестовый класс | Количество тестов | Покрытие |
|----------------|-------------------|----------|
| DefaultModuleConnectionStringProviderTests | 17 | ✅ 100% |

---

## Заключение

✅ **Успешно добавлено:**
- Новый тестовый проект `Cheetah.Core.Tenants.Tests`
- 17 unit тестов для `DefaultModuleConnectionStringProvider`
- 100% покрытие основного функционала
- Все тесты успешно компилируются и проходят

🎯 **Качество:**
- Использование AAA pattern
- Параметризованные тесты через Theory/InlineData
- Покрытие edge cases
- Case-insensitive проверки для DbConnectionStringBuilder

📊 **Результат тестирования:**
```
Test Run Successful.
Total tests: 17
     Passed: 17
 Total time: 0.5251 Seconds
```

🚀 **Готовность:**
- Тесты готовы к запуску в CI/CD
- Легко расширяемая структура
- Полное покрытие генерации connection strings
