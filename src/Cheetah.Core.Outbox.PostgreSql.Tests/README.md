# Cheetah.Core.Outbox.PostgreSql.Tests

Unit-тесты для [Cheetah.Core.Outbox.PostgreSql](../Cheetah.Core.Outbox.PostgreSql/README.md).

## Покрытие

**OutboxNotifyTriggerSqlTests**
- `Create()` содержит `pg_notify`, имя канала и корректное имя триггера
- `Drop()` удаляет триггер и функцию
- Небезопасные имена каналов (`'bad name'`, `name;DROP`, `'injection'`) отвергаются

Интеграционные тесты на сам LISTEN-цикл не входят — они требуют реального Postgres (`Testcontainers.PostgreSql`).

## Запуск

```bash
dotnet test src/Cheetah.Core.Outbox.PostgreSql.Tests/Cheetah.Core.Outbox.PostgreSql.Tests.csproj
```
