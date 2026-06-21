using Cheetah.Modules.CustomFields.Domain.Entities;
using Cheetah.Modules.CustomFields.Domain.Specifications;
using Cheetah.Modules.CustomFields.DomainEvents;
using Cheetah.Modules.CustomFields.Shared;
using Shouldly;

namespace Cheetah.Modules.CustomFields.Domain.Tests;

public class CustomFieldValueSetTests
{
    [Fact]
    public void Create_RaisesValuesChanged()
    {
        var set = CustomFieldValueSet.Create(null, "crm.deal", "d1", "{\"a\":1}");

        set.ValuesJson.ShouldBe("{\"a\":1}");
        set.DomainEvents.OfType<CustomFieldValuesChangedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Replace_UpdatesJson_AndRaisesValuesChanged()
    {
        var set = CustomFieldValueSet.Create(null, "crm.deal", "d1", "{}");
        set.Replace("{\"b\":2}");

        set.ValuesJson.ShouldBe("{\"b\":2}");
        set.DomainEvents.OfType<CustomFieldValuesChangedIntegrationEvent>().Count().ShouldBe(2);
    }

    [Fact]
    public void ValueSetsByEntityIds_Spec_MatchesIdsOfType()
    {
        var ids = new[] { "a", "b" };
        var spec = new ValueSetsByEntityIdsSpecification(null, "crm.deal", ids);
        var predicate = spec.ToExpression().Compile();

        predicate(CustomFieldValueSet.Create(null, "crm.deal", "a", "{}")).ShouldBeTrue();
        predicate(CustomFieldValueSet.Create(null, "crm.deal", "c", "{}")).ShouldBeFalse();
        predicate(CustomFieldValueSet.Create(null, "crm.contact", "a", "{}")).ShouldBeFalse();
    }

    [Fact]
    public void DefinitionsByEntityType_Spec_IncludesGlobalAndTenant()
    {
        var tenant = Guid.NewGuid();
        var spec = new DefinitionsByEntityTypeSpecification(tenant, "crm.deal", onlyActive: true);
        var predicate = spec.ToExpression().Compile();

        var global = CustomFieldDefinition.Create(null, "crm.deal", "g", "G",
            CustomFieldDataType.String, false, null, null, null, 0);
        var ofTenant = CustomFieldDefinition.Create(tenant, "crm.deal", "t", "T",
            CustomFieldDataType.String, false, null, null, null, 0);
        var otherTenant = CustomFieldDefinition.Create(Guid.NewGuid(), "crm.deal", "o", "O",
            CustomFieldDataType.String, false, null, null, null, 0);

        predicate(global).ShouldBeTrue();
        predicate(ofTenant).ShouldBeTrue();
        predicate(otherTenant).ShouldBeFalse();
    }
}
