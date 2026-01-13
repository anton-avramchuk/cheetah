# Cheetah.Analyzers

Roslyn analyzer для проверки соответствия между ProjectReference и атрибутами [DependsOn] в модулях Cheetah CRM.

## Возможности

### CHT001: Missing module dependency attribute

**Ошибка компиляции** - возникает, когда проект ссылается на другой проект с модулем, но не имеет соответствующего `[DependsOn]` атрибута.

**Пример:**

```csharp
// ❌ ОШИБКА: Project references 'Cheetah.Tenants.Domain' but module 'CrmTenantsApiModule' is missing [DependsOn(typeof(CrmTenantsDomainModule))]
public partial class CrmTenantsApiModule : CrmModule
{
    // ...
}
```

**Исправление (вручную):**

```csharp
// ✅ Правильно
[DependsOn(typeof(CrmTenantsDomainModule))]
public partial class CrmTenantsApiModule : CrmModule
{
    // ...
}
```

**Code Fix:** Анализатор предлагает автоматическое добавление недостающего атрибута `[DependsOn]`.

### CHT002: Unused module dependency attribute

**Предупреждение** - возникает, когда модуль имеет `[DependsOn]` атрибут, но проект не ссылается на соответствующий проект.

**Пример:**

```csharp
// ⚠️ ПРЕДУПРЕЖДЕНИЕ: Module 'CrmTenantsApiModule' has [DependsOn(typeof(CrmMapsterModule))] but project doesn't reference 'Cheetah.Mapping.Mapster'
[DependsOn(typeof(CrmMapsterModule))]
public partial class CrmTenantsApiModule : CrmModule
{
    // ...
}
```

**Исправление:**

Либо добавить ProjectReference:

```xml
<ItemGroup>
  <ProjectReference Include="..\Cheetah.Mapping.Mapster\Cheetah.Mapping.Mapster.csproj" />
</ItemGroup>
```

Либо удалить неиспользуемый атрибут (CodeFix предложит автоматическое удаление).

## Использование

### Автоматическое подключение (рекомендуется)

Добавьте в `Directory.Build.props` в корне решения:

```xml
<Project>
  <ItemGroup>
    <ProjectReference Include="$(MSBuildThisFileDirectory)src\Cheetah.Analyzers\Cheetah.Analyzers.csproj"
                      ReferenceOutputAssembly="false"
                      OutputItemType="Analyzer"/>
  </ItemGroup>
</Project>
```

Теперь все проекты в решении будут автоматически проверяться анализатором.

### Исключить проект из проверки

```xml
<PropertyGroup>
  <EnableCheetahAnalyzers>false</EnableCheetahAnalyzers>
</PropertyGroup>
```

### Ручное подключение

В конкретном проекте:

```xml
<ItemGroup>
  <ProjectReference Include="..\Cheetah.Analyzers\Cheetah.Analyzers.csproj"
                    ReferenceOutputAssembly="false"
                    OutputItemType="Analyzer"/>
</ItemGroup>
```

## Работает везде

- ✅ Visual Studio 2022
- ✅ Visual Studio Code
- ✅ JetBrains Rider
- ✅ `dotnet build` (CLI)
- ✅ CI/CD pipelines

## Преимущества

1. **Автоматическая проверка** - ошибки обнаруживаются на этапе компиляции
2. **Code Fix** - автоматическое исправление большинства проблем
3. **IDE интеграция** - подсветка и исправление прямо в редакторе
4. **CI/CD** - предотвращает merge невалидного кода
5. **Без установки** - все разработчики команды получают проверки автоматически

## Разработка

### Запуск тестов

```bash
dotnet test src/Cheetah.Analyzers.Tests/Cheetah.Analyzers.Tests.csproj
```

### Отладка

1. Установите проект с анализатором как Startup Project
2. В свойствах проекта Debug → Start external program: укажите путь к `devenv.exe` (Visual Studio)
3. Запустите отладку (F5)
4. Откроется экспериментальный экземпляр Visual Studio с вашим анализатором

## Архитектура

- **ModuleDependencyAnalyzer** - основной анализатор, выполняет проверку на CompilationEnd
- **ModuleDependencyCodeFixProvider** - предоставляет автоматические исправления
- **DiagnosticDescriptors** - определения правил CHT001 и CHT002
