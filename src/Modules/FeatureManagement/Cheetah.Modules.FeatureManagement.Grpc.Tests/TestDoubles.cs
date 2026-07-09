using Cheetah.Core.CQRS;
using Cheetah.FeatureManagement;
using Grpc.Core;

namespace Cheetah.Modules.FeatureManagement.Grpc.Tests;

/// <summary>Минимальный ServerCallContext для юнит-тестов сервиса (без Kestrel/канала).</summary>
internal sealed class TestServerCallContext : ServerCallContext
{
    private readonly CancellationToken _cancellationToken;

    public TestServerCallContext(CancellationToken cancellationToken = default)
        => _cancellationToken = cancellationToken;

    protected override string MethodCore => "/cheetah.modules.featuremanagement.v1.FeatureCatalog/Test";
    protected override string HostCore => "localhost";
    protected override string PeerCore => "ipv4:127.0.0.1";
    protected override DateTime DeadlineCore => DateTime.MaxValue;
    protected override Metadata RequestHeadersCore { get; } = new();
    protected override CancellationToken CancellationTokenCore => _cancellationToken;
    protected override Metadata ResponseTrailersCore { get; } = new();
    protected override Status StatusCore { get; set; }
    protected override WriteOptions? WriteOptionsCore { get; set; }
    protected override AuthContext AuthContextCore { get; } =
        new(null, new Dictionary<string, List<AuthProperty>>());

    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
        => throw new NotSupportedException();

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        => Task.CompletedTask;
}

/// <summary>Диспетчер, отвечающий фиксированным результатом EvaluateFeaturesQuery и запоминающий запрос.</summary>
internal sealed class FakeDispatcher : IDispatcher
{
    public object? LastQuery { get; private set; }
    public IReadOnlyDictionary<string, Contracts.FeatureEvaluationDto> EvaluationResult { get; init; }
        = new Dictionary<string, Contracts.FeatureEvaluationDto>();

    public ValueTask SendAsync<TCommand>(TCommand command, CancellationToken ct = default)
        where TCommand : ICommand
        => throw new NotSupportedException();

    public ValueTask<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken ct = default)
        where TCommand : ICommand<TResult>
        => throw new NotSupportedException();

    public ValueTask<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken ct = default)
        where TQuery : IQuery<TResult>
    {
        LastQuery = query;
        return ValueTask.FromResult((TResult)(object)EvaluationResult);
    }
}

/// <summary>Провайдер с фиксированным набором определений.</summary>
internal sealed class FakeDefinitionProvider(params FeatureDefinition[] definitions) : IFeatureDefinitionProvider
{
    public Guid? LastTenantId { get; private set; }

    public ValueTask<FeatureDefinition?> GetAsync(string featureKey, Guid? tenantId, CancellationToken ct = default)
        => ValueTask.FromResult(definitions.FirstOrDefault(d => d.Key == featureKey));

    public ValueTask<IReadOnlyList<FeatureDefinition>> GetAllAsync(Guid? tenantId, CancellationToken ct = default)
    {
        LastTenantId = tenantId;
        return ValueTask.FromResult<IReadOnlyList<FeatureDefinition>>(definitions);
    }
}
