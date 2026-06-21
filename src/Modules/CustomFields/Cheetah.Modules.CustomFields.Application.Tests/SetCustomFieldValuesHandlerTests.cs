using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.CustomFields.Application.Mapping;
using Cheetah.Modules.CustomFields.Application.Values;
using Cheetah.Modules.CustomFields.Contracts;
using Cheetah.Modules.CustomFields.Domain.Abstractions;
using Cheetah.Modules.CustomFields.Domain.Entities;
using Cheetah.Modules.CustomFields.Shared;
using Cheetah.Validation;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;

namespace Cheetah.Modules.CustomFields.Application.Tests;

public class SetCustomFieldValuesHandlerTests
{
    private static CustomFieldDefinitionSnapshot Field(string key, bool required = false,
        string? rulesJson = null, CustomFieldDataType type = CustomFieldDataType.String)
        => new(Guid.NewGuid(), null, "crm.deal", key, key, type, required, null, rulesJson, null, 0);

    private static (SetCustomFieldValuesCommandHandler handler, Mock<IRepository<CustomFieldValueSet, Guid>> sets,
        Mock<IEventBus> bus, List<CustomFieldValueSet> added)
        Build(IReadOnlyList<CustomFieldDefinitionSnapshot> defs)
    {
        var reader = new Mock<ICustomFieldDefinitionReader>();
        reader.Setup(r => r.GetActiveAsync(It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(defs);

        var sets = new Mock<IRepository<CustomFieldValueSet, Guid>>();
        var added = new List<CustomFieldValueSet>();
        sets.Setup(s => s.GetBySpecAsync(It.IsAny<Cheetah.Core.Specification.ISpecification<CustomFieldValueSet>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomFieldValueSet?)null);
        sets.Setup(s => s.Add(It.IsAny<CustomFieldValueSet>())).Callback<CustomFieldValueSet>(added.Add);
        sets.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var bus = new Mock<IEventBus>();
        bus.Setup(b => b.PublishAsync(It.IsAny<IEvent>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var mapper = new CustomFieldDefinitionMapper(new ValidationRuleSerializer());
        var engine = new ValidationEngine();
        var services = new ServiceCollection().BuildServiceProvider();

        var handler = new SetCustomFieldValuesCommandHandler(
            reader.Object, sets.Object, mapper, engine, services, bus.Object);
        return (handler, sets, bus, added);
    }

    [Fact]
    public async Task ValidValues_SavesAndPublishes()
    {
        var (handler, sets, bus, added) = Build([Field("delivery_terms", required: true)]);
        var cmd = new SetCustomFieldValuesCommand(new SetCustomFieldValuesRequest(
            "crm.deal", "d1", new Dictionary<string, object?> { ["delivery_terms"] = "DAP" }));

        await handler.HandleAsync(cmd);

        sets.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        bus.Verify(b => b.PublishAsync(It.IsAny<IEvent>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        added.ShouldHaveSingleItem();
        added[0].ValuesJson.ShouldContain("DAP");
    }

    [Fact]
    public async Task MissingRequired_ThrowsWithError()
    {
        var (handler, _, _, _) = Build([Field("delivery_terms", required: true)]);
        var cmd = new SetCustomFieldValuesCommand(new SetCustomFieldValuesRequest(
            "crm.deal", "d1", new Dictionary<string, object?>()));

        var ex = await Should.ThrowAsync<CustomFieldsValidationException>(() => handler.HandleAsync(cmd).AsTask());
        ex.Errors.ShouldContain(e => e.FieldPath == "delivery_terms");
    }

    [Fact]
    public async Task StripsUnregisteredKeys()
    {
        var (handler, _, _, added) = Build([Field("a")]);
        var cmd = new SetCustomFieldValuesCommand(new SetCustomFieldValuesRequest(
            "crm.deal", "d1", new Dictionary<string, object?> { ["a"] = "x", ["unknown"] = "y" }));

        await handler.HandleAsync(cmd);

        added[0].ValuesJson.ShouldContain("\"a\"");
        added[0].ValuesJson.ShouldNotContain("unknown");
    }
}
