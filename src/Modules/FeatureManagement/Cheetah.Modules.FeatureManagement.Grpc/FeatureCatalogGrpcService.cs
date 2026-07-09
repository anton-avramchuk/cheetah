using Cheetah.Core.CQRS;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Application.Queries;
using Cheetah.Modules.FeatureManagement.Contracts;
using Cheetah.Modules.FeatureManagement.Grpc.Protos;
using Grpc.Core;

namespace Cheetah.Modules.FeatureManagement.Grpc;

/// <summary>
/// Тонкий gRPC-сервис горячего пути: транслирует proto-вызовы в те же CQRS-запросы/порт, что и REST
/// (<c>/api/features/evaluate</c> и <c>/api/features/definitions</c>). Бизнес-логика не дублируется.
/// Ошибки транслирует <c>CrmExceptionInterceptor</c> (Backend.Grpc).
/// В DI не регистрируется — <c>MapGrpcService&lt;T&gt;()</c> создаёт его через DI сам.
/// </summary>
public sealed class FeatureCatalogGrpcService : FeatureCatalog.FeatureCatalogBase
{
    private readonly IDispatcher _dispatcher;
    private readonly IFeatureDefinitionProvider _provider;

    public FeatureCatalogGrpcService(IDispatcher dispatcher, IFeatureDefinitionProvider provider)
    {
        _dispatcher = dispatcher;
        _provider = provider;
    }

    public override async Task<EvaluateFeaturesProtoReply> Evaluate(
        EvaluateFeaturesProtoRequest request, ServerCallContext context)
    {
        var featureContext = GrpcFeatureConverters.ToContext(request.Context);
        var evaluations = await _dispatcher
            .QueryAsync<EvaluateFeaturesQuery, IReadOnlyDictionary<string, FeatureEvaluationDto>>(
                new EvaluateFeaturesQuery(request.Keys, featureContext), context.CancellationToken);

        var reply = new EvaluateFeaturesProtoReply();
        foreach (var (key, evaluation) in evaluations)
        {
            var proto = new FeatureEvaluationProto { Enabled = evaluation.Enabled };
            if (evaluation.Variant is not null)
                proto.Variant = evaluation.Variant;
            reply.Results[key] = proto;
        }

        return reply;
    }

    public override async Task<FeatureDefinitionsProtoReply> PullDefinitions(
        PullDefinitionsProtoRequest request, ServerCallContext context)
    {
        Guid? tenantId = request.HasTenantId && request.TenantId.Length > 0
            ? Guid.Parse(request.TenantId)
            : null;

        var definitions = await _provider.GetAllAsync(tenantId, context.CancellationToken);

        var reply = new FeatureDefinitionsProtoReply();
        foreach (var definition in definitions)
            reply.Definitions.Add(GrpcFeatureConverters.ToProto(definition));

        return reply;
    }
}
