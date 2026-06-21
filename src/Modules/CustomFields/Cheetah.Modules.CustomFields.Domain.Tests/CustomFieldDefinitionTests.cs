using Cheetah.Modules.CustomFields.Domain.Entities;
using Cheetah.Modules.CustomFields.DomainEvents;
using Cheetah.Modules.CustomFields.Shared;
using Shouldly;

namespace Cheetah.Modules.CustomFields.Domain.Tests;

public class CustomFieldDefinitionTests
{
    private static CustomFieldDefinition NewString(Guid? tenant = null)
        => CustomFieldDefinition.Create(tenant, "crm.deal", "delivery_terms", "Условия доставки",
            CustomFieldDataType.String, required: false, options: null,
            validationRulesJson: null, visibilityRule: null, order: 1);

    [Fact]
    public void Create_RaisesCreatedEvent_AndIsActive()
    {
        var d = NewString();

        d.IsActive.ShouldBeTrue();
        d.Key.ShouldBe("delivery_terms");
        d.DomainEvents.OfType<CustomFieldDefinitionCreatedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Create_EnumWithoutOptions_Throws()
    {
        Should.Throw<InvalidOperationException>(() =>
            CustomFieldDefinition.Create(null, "crm.deal", "priority", "Приоритет",
                CustomFieldDataType.Enum, required: false, options: null,
                validationRulesJson: null, visibilityRule: null, order: 0));
    }

    [Fact]
    public void Create_EnumWithOptions_Succeeds()
    {
        var d = CustomFieldDefinition.Create(null, "crm.deal", "priority", "Приоритет",
            CustomFieldDataType.Enum, required: false, options: ["low", "high"],
            validationRulesJson: null, visibilityRule: null, order: 0);

        d.Options.ShouldBe(["low", "high"]);
    }

    [Fact]
    public void UpdateMetadata_RaisesChanged_AndKeepsDataType()
    {
        var d = NewString();
        d.UpdateMetadata("Новый лейбл", required: true, options: null,
            validationRulesJson: "[]", visibilityRule: "{\"==\":[1,1]}", order: 5);

        d.Label.ShouldBe("Новый лейбл");
        d.Required.ShouldBeTrue();
        d.Order.ShouldBe(5);
        d.DataType.ShouldBe(CustomFieldDataType.String);
        d.DomainEvents.OfType<CustomFieldDefinitionChangedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Deactivate_IsIdempotent_AndRaisesEventOnce()
    {
        var d = NewString();
        d.Deactivate();
        d.Deactivate();

        d.IsActive.ShouldBeFalse();
        d.DomainEvents.OfType<CustomFieldDefinitionDeactivatedIntegrationEvent>().Count().ShouldBe(1);
    }
}
