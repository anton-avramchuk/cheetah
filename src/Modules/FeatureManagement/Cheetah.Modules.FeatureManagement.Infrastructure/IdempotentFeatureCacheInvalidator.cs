using Cheetah.Core.Events;
using Cheetah.Core.Inbox;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.FeatureManagement.Infrastructure;

/// <summary>
/// Идемпотентная подписка <see cref="FeatureCacheInvalidator"/> на шину: оборачивает
/// <see cref="InboxIdempotentEventHandler{TEvent}"/> и сама коммитит inbox-запись
/// (<see cref="DbContext.SaveChangesAsync(CancellationToken)"/>) — сам инвалидатор не пишет в БД
/// (только <c>ICacheService</c>), поэтому сохранить запись Inbox некому, кроме этой подписки.
/// <para>
/// Регистрируется как закрытый generic-тип (см. <c>AddFeatureManagementInfrastructure</c>) и
/// подписывается через <c>IEventBus.Subscribe&lt;TEvent, THandler&gt;()</c> — шина резолвит хендлер
/// по конкретному типу <typeparamref name="TEvent"/>/<typeparamref name="TContext"/>, а не по
/// <see cref="IEventHandler{TEvent}"/>, поэтому декоратор нельзя подключить только через DI-регистрацию
/// интерфейса — здесь он подписывается напрямую.
/// </para>
/// </summary>
public sealed class IdempotentFeatureCacheInvalidator<TContext, TEvent> : IEventHandler<TEvent>
    where TContext : DbContext
    where TEvent : IEvent
{
    private readonly InboxIdempotentEventHandler<TEvent> _inner;
    private readonly TContext _context;

    public IdempotentFeatureCacheInvalidator(InboxIdempotentEventHandler<TEvent> inner, TContext context)
    {
        _inner = inner;
        _context = context;
    }

    public async ValueTask HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
    {
        await _inner.HandleAsync(@event, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
