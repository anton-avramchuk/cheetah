# Этап 3. `TenantId` в событиях и четырёх шинах

> [← Бэклог](README.md) · Зависимости: [T-1.1](stage-1-current-tenant.md#t-11).
> Открывает путь этапу 8 (фоновые потоки).

<a id="t-31"></a>
## T-3.1 `EventBase.TenantId` + хелперы стемпинга/скоупа — **S**

**Зависит от:** T-1.1.
**Файлы:** `src/Cheetah.Core.Events/EventBase.cs`;
`src/Cheetah.Core.Tenants/Events/TenantEventExtensions.cs`.

**Шаги:**
1. В `EventBase`: `public Guid? TenantId { get; set; }` — с XML-doc «заполняется шиной
   автоматически из ICurrentTenant; вручную задавать только при публикации от имени
   другого тенанта». `set` (а не `init`) — осознанно: шина стемпит уже созданный record.
2. Хелперы в `Cheetah.Core.Tenants` (шины получат зависимость на неё — она легковесна):
   ```csharp
   public static class TenantEventExtensions
   {
       // если TenantId == null и тенант доступен — проставить; вернуть событие
       public static TEvent StampTenant<TEvent>(this TEvent e, ICurrentTenant t) where TEvent : IEvent;
       // scope для консюмера: Change(TenantId события); для не-EventBase — no-op
       public static IDisposable EnterTenantScope(this IEvent e, ICurrentTenant t);
   }
   ```
   `EnterTenantScope` для события с `TenantId == null` обязан делать `Change(null)`
   (а НЕ no-op при доступном тенанте): хост-событие, обработанное в тенант-контексте,
   не должно унаследовать чужого тенанта.
3. Юнит-тесты: стемпинг при доступном/недоступном тенанте; уже проставленный TenantId
   не перезаписывается; `EnterTenantScope` восстанавливает предыдущий контекст.

**DoD:** поле + хелперы + тесты; сериализация (JSON) подхватывает поле автоматически — проверить тестом.

<a id="t-32"></a>
## T-3.2 Стемпинг и скоуп во всех реализациях `IEventBus` — **M**

**Зависит от:** T-3.1.
**Файлы:** `src/Cheetah.Backend.Events.Redis/CrmRedisEventBus.cs`,
`src/Cheetah.Backend.Events.Kafka/CrmKafkaEventBus.cs`,
`src/Cheetah.Backend.Events.InMemory/InMemoryEventBus.cs`,
`src/Cheetah.Core.Outbox/OutboxEventBus.cs` + их модули (`[DependsOn(typeof(CrmTenantsCoreModule))]`)
и csproj-референсы.

**Шаги (для каждой шины):**
1. Внедрить `ICurrentTenant`.
2. `PublishAsync`/`PublishManyAsync`: `@event.StampTenant(_currentTenant)` **до** сериализации.
   Для `OutboxEventBus` критично: стемпить до записи строки в outbox-таблицу (конверт в БД
   уже должен нести TenantId; при последующей отправке из outbox-диспетчера амбиентного
   тенанта не будет).
3. Сторона доставки (`HandleEventAsync` и аналоги): обернуть вызов хендлеров:
   `using (@event.EnterTenantScope(_currentTenant)) { ...все хендлеры... }`.
4. Kafka: опционально (флагом в опциях) использовать `TenantId` как ключ партиционирования —
   даёт упорядоченность событий внутри тенанта. По умолчанию выключено, отдельная опция.
5. Тесты на каждую шину (в существующих `*.Tests` проектах):
   - publish в тенант-контексте → подписчик получил событие с TenantId и **выполнился в
     контексте этого тенанта** (ассерт через захват `ICurrentTenant.Id` внутри хендлера);
   - publish без тенанта → TenantId == null, хендлер в хост-контексте;
   - для Outbox: строка в таблице содержит TenantId в payload.

**DoD:** все 4 шины стемпят и восстанавливают контекст; тесты зелёные; README шин обновлены.
