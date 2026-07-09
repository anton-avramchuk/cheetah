using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Application.Queries;
using Cheetah.Modules.FeatureManagement.Contracts;
using Cheetah.Modules.FeatureManagement.Grpc.Protos;
using Shouldly;

namespace Cheetah.Modules.FeatureManagement.Grpc.Tests;

public class FeatureCatalogGrpcServiceTests
{
    [Fact]
    public async Task Evaluate_translates_context_and_returns_results()
    {
        var userId = Guid.NewGuid();
        var dispatcher = new FakeDispatcher
        {
            EvaluationResult = new Dictionary<string, FeatureEvaluationDto>
            {
                ["deals.kanban-v2"] = new(true, null),
                ["pricing.experiment"] = new(true, "treatment")
            }
        };
        var service = new FeatureCatalogGrpcService(dispatcher, new FakeDefinitionProvider());

        var request = new EvaluateFeaturesProtoRequest
        {
            Keys = { "deals.kanban-v2", "pricing.experiment" },
            Context = new FeatureContextProto
            {
                UserId = userId.ToString(),
                Roles = { "manager" },
                AttributesJson = { ["planTier"] = "\"pro\"", ["seats"] = "42" }
            }
        };
        var reply = await service.Evaluate(request, new TestServerCallContext());

        // Контекст дошёл до CQRS-запроса в типах движка.
        var query = dispatcher.LastQuery.ShouldBeOfType<EvaluateFeaturesQuery>();
        query.Context.UserId.ShouldBe(userId);
        query.Context.Roles.ShouldBe(["manager"]);
        query.Context.Attributes["planTier"].ShouldNotBeNull();

        reply.Results["deals.kanban-v2"].Enabled.ShouldBeTrue();
        reply.Results["deals.kanban-v2"].HasVariant.ShouldBeFalse();
        reply.Results["pricing.experiment"].Variant.ShouldBe("treatment");
    }

    [Fact]
    public async Task PullDefinitions_returns_snapshot_with_rules_and_variants()
    {
        var definition = new FeatureDefinition(
            "pricing.experiment", Enabled: true, FeatureValueType.Variant,
            [new TargetingRuleDefinition(0, "Percentage",
                new Dictionary<string, object?> { ["percentage"] = 25 }, "treatment")],
            [new VariantDefinition("control", null, 50), new VariantDefinition("treatment", "v2", 50)]);
        var provider = new FakeDefinitionProvider(definition);
        var service = new FeatureCatalogGrpcService(new FakeDispatcher(), provider);

        var tenantId = Guid.NewGuid();
        var reply = await service.PullDefinitions(
            new PullDefinitionsProtoRequest { TenantId = tenantId.ToString() }, new TestServerCallContext());

        provider.LastTenantId.ShouldBe(tenantId);
        var proto = reply.Definitions.ShouldHaveSingleItem();
        proto.Key.ShouldBe("pricing.experiment");
        proto.Enabled.ShouldBeTrue();
        proto.ValueType.ShouldBe((int)FeatureValueType.Variant);
        proto.Rules.ShouldHaveSingleItem().ParametersJson["percentage"].ShouldBe("25");
        proto.Variants.Count.ShouldBe(2);
        proto.Variants[0].HasValue.ShouldBeFalse(); // null Value не передаётся
    }

    [Fact]
    public void Definition_roundtrips_through_proto()
    {
        var original = new FeatureDefinition(
            "deals.kanban-v2", Enabled: true, FeatureValueType.Bool,
            [new TargetingRuleDefinition(1, "Users",
                new Dictionary<string, object?> { ["users"] = new[] { "a", "b" } }, null, Negate: true)],
            []);

        var restored = GrpcFeatureConverters.ToDefinition(GrpcFeatureConverters.ToProto(original));

        restored.Key.ShouldBe(original.Key);
        restored.Enabled.ShouldBeTrue();
        var rule = restored.Rules.ShouldHaveSingleItem();
        rule.FilterName.ShouldBe("Users");
        rule.Negate.ShouldBeTrue();
        rule.ResultVariant.ShouldBeNull();
        rule.Parameters["users"].ShouldNotBeNull();
    }

    [Fact]
    public void Empty_context_maps_to_empty_feature_context()
    {
        var context = GrpcFeatureConverters.ToContext(null);
        context.ShouldBe(FeatureContext.Empty);

        var explicitEmpty = GrpcFeatureConverters.ToContext(new FeatureContextProto());
        explicitEmpty.UserId.ShouldBeNull();
        explicitEmpty.TenantId.ShouldBeNull();
        explicitEmpty.Roles.ShouldBeEmpty();
        explicitEmpty.Attributes.ShouldBeEmpty();
    }
}
