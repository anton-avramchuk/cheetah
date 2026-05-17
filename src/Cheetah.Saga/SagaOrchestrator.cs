using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cheetah.Saga;

[Export(LifetimeType.Scoped, typeof(ISagaOrchestrator))]
public sealed class SagaOrchestrator : ISagaOrchestrator
{
    private readonly IServiceProvider _services;
    private readonly SagaRegistry _registry;
    private readonly ISagaRepository _repository;
    private readonly ILogger<SagaOrchestrator> _logger;

    public SagaOrchestrator(
        IServiceProvider services,
        SagaRegistry registry,
        ISagaRepository repository,
        ILogger<SagaOrchestrator> logger)
    {
        _services = services;
        _registry = registry;
        _repository = repository;
        _logger = logger;
    }

    public async ValueTask HandleAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        var eventType = @event.GetType();
        var matches = _registry.ForEvent(eventType).ToList();
        if (matches.Count == 0) return;

        foreach (var reg in matches)
        {
            await DispatchToSagaAsync(reg, @event, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task DispatchToSagaAsync(SagaRegistration reg, IEvent @event, CancellationToken ct)
    {
        var sagaTypeName = reg.SagaType.FullName!;

        // Создаём временный экземпляр для извлечения CorrelationKey
        // (GetCorrelation не должен трогать Data, поэтому без репозитория).
        var probe = (ISaga)ActivatorUtilities.CreateInstance(_services, reg.SagaType);
        var correlationKey = probe.GetCorrelation(@event);

        var instance = await _repository.FindAsync(sagaTypeName, correlationKey, ct).ConfigureAwait(false);

        ISaga saga;
        bool isNew;
        if (instance is null)
        {
            if (!reg.CanStart(@event.GetType()))
            {
                _logger.LogWarning("Saga {SagaType}/{Correlation} not found and event {Event} cannot start a new instance — ignoring",
                    sagaTypeName, correlationKey, @event.GetType().Name);
                return;
            }
            instance = new SagaInstance
            {
                SagaType = sagaTypeName,
                CorrelationKey = correlationKey,
                DataType = probe.DataType.AssemblyQualifiedName!,
                DataJson = SagaSerializer.Serialize(Activator.CreateInstance(probe.DataType)!, out _)
            };
            saga = probe;
            isNew = true;
        }
        else
        {
            // Резолвим новый instance через DI (probe был только для GetCorrelation).
            saga = (ISaga)ActivatorUtilities.CreateInstance(_services, reg.SagaType);
            saga.Data = SagaSerializer.Deserialize(instance.DataJson, instance.DataType);
            isNew = false;

            if (instance.Status is SagaStatus.Completed or SagaStatus.Compensated or SagaStatus.Failed)
            {
                _logger.LogDebug("Saga {SagaType}/{Correlation} already in terminal state {Status} — ignoring event {Event}",
                    sagaTypeName, correlationKey, instance.Status, @event.GetType().Name);
                return;
            }
        }

        try
        {
            await saga.HandleAsync(@event, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Saga {SagaType}/{Correlation} handler threw on event {Event}",
                sagaTypeName, correlationKey, @event.GetType().Name);
            saga.Fail(ex.Message);
        }

        // Снимаем данные обратно
        instance.DataJson = SagaSerializer.Serialize(saga.Data, out _);
        instance.Status = saga.Status;
        instance.Reason = saga.Reason ?? instance.Reason;
        instance.UpdatedAt = DateTimeOffset.UtcNow;

        if (isNew)
        {
            await _repository.AddAsync(instance, ct).ConfigureAwait(false);
        }
        await _repository.SaveChangesAsync(ct).ConfigureAwait(false);

        _logger.LogDebug("Saga {SagaType}/{Correlation} → {Status} after {Event}",
            sagaTypeName, correlationKey, instance.Status, @event.GetType().Name);
    }
}
