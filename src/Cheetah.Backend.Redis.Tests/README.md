# Cheetah.Backend.Redis.Tests

Unit тесты для модуля Cheetah.Backend.Redis.

## Структура тестов

### RedisConnectionProviderTests
Тесты для провайдера подключений к Redis:
- Проверка получения подключений к разным инстансам
- Проверка получения баз данных
- Проверка обработки несуществующих инстансов
- Проверка переиспользования подключений

**Примечание:** Тесты делятся на две категории:
- **Unit тесты** - используют моки, не требуют Redis сервера
- **Интеграционные тесты** - помечены `[Trait("Category", "Integration")]`, требуют запущенного Redis сервера

### RedisClientTests
Unit тесты для клиента Redis с использованием моков:
- Тестирование CRUD операций (Get, Set, Delete, Exists)
- Тестирование работы с TTL
- Тестирование batch операций (GetMany, SetMany)
- Тестирование использования разных инстансов Redis
- Всего: **13 тестов**

### RedisEventBusTests
Unit тесты для шины событий Redis с использованием моков:
- Тестирование публикации событий
- Тестирование подписки на события
- Тестирование обработки событий
- Тестирование отписки от событий
- Тестирование работы с несколькими обработчиками
- Тестирование использования разных инстансов Redis
- Всего: **9 тестов**

## Запуск тестов

```bash
# Запустить только unit тесты (рекомендуется для CI/CD)
dotnet test --filter "Category!=Integration"

# Запустить все тесты (включая интеграционные, требуется Redis)
dotnet test

# Запустить только интеграционные тесты
dotnet test --filter "Category=Integration"

# Запустить с подробным выводом
dotnet test --logger "console;verbosity=normal"
```

## Интеграционные тесты

Для запуска интеграционных тестов необходим запущенный сервер Redis:

```bash
# Запустить Redis в Docker
docker run -d -p 6379:6379 redis:latest

# Запустить все тесты (включая интеграционные)
dotnet test

# Или только интеграционные
dotnet test --filter "Category=Integration"
```

## GitHub Actions / CI

В CI/CD пайплайнах рекомендуется запускать только unit тесты:

```yaml
- name: Run tests
  run: dotnet test --filter "Category!=Integration"
```

## Покрытие кода

Все основные сценарии работы с Redis покрыты тестами:
- ✅ Работа с данными (Get/Set/Delete)
- ✅ Поиск ключей
- ✅ Batch операции
- ✅ Pub/Sub события
- ✅ Обработка ошибок
- ✅ Работа с несколькими инстансами

## Используемые библиотеки

- **xUnit** - фреймворк для тестирования
- **FluentAssertions** - библиотека для читаемых assertions
- **Moq** - библиотека для создания моков
- **Microsoft.Extensions.Options** - для конфигурации

## Результаты

**Unit тесты (без Redis сервера):**
```
dotnet test --filter "Category!=Integration"
Total tests: 20
     Passed: 20
```

**Все тесты (с Redis сервером):**
```
dotnet test
Total tests: 25
     Passed: 25
```
