using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Core.Specification;
using Cheetah.Modules.CustomFields.Application.Registry;
using Cheetah.Modules.CustomFields.Application.Values;
using Cheetah.Modules.CustomFields.Contracts;
using Cheetah.Modules.CustomFields.Domain.Entities;
using Cheetah.Modules.CustomFields.Shared;
using Moq;
using Shouldly;

namespace Cheetah.Modules.CustomFields.Application.Tests;

public class RegistryAndQueryTests
{
    [Fact]
    public async Task SyncRegistry_CreatesType_AndPredefinedGlobalField()
    {
        var types = new Mock<IRepository<CustomFieldEntityType, Guid>>();
        var defs = new Mock<IRepository<CustomFieldDefinition, Guid>>();
        var bus = new Mock<IEventBus>();
        bus.Setup(b => b.PublishAsync(It.IsAny<IEvent>(), It.IsAny<CancellationToken>())).Returns(ValueTask.CompletedTask);

        types.Setup(t => t.GetBySpecAsync(It.IsAny<ISpecification<CustomFieldEntityType>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomFieldEntityType?)null);
        types.Setup(t => t.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        // Поле ещё не существует → должно быть создано.
        defs.Setup(d => d.ExistsAsync(It.IsAny<ISpecification<CustomFieldDefinition>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var addedTypes = new List<CustomFieldEntityType>();
        var addedDefs = new List<CustomFieldDefinition>();
        types.Setup(t => t.Add(It.IsAny<CustomFieldEntityType>())).Callback<CustomFieldEntityType>(addedTypes.Add);
        defs.Setup(d => d.Add(It.IsAny<CustomFieldDefinition>())).Callback<CustomFieldDefinition>(addedDefs.Add);

        var handler = new SyncEntityTypeRegistryCommandHandler(types.Object, defs.Object, bus.Object);
        var cmd = new SyncEntityTypeRegistryCommand([
            new CustomFieldEntityTypeDescriptor("crm.deal", "Сделка", "deals", CustomFieldEntityIdType.Guid,
                PredefinedFields: [new PredefinedFieldDescriptor("industry", "Отрасль", CustomFieldDataType.String)])
        ]);

        await handler.HandleAsync(cmd);

        addedTypes.ShouldHaveSingleItem();
        addedDefs.ShouldHaveSingleItem();
        addedDefs[0].Key.ShouldBe("industry");
    }

    [Fact]
    public async Task SyncRegistry_ExistingField_NotDuplicated()
    {
        var types = new Mock<IRepository<CustomFieldEntityType, Guid>>();
        var defs = new Mock<IRepository<CustomFieldDefinition, Guid>>();
        var bus = new Mock<IEventBus>();
        bus.Setup(b => b.PublishAsync(It.IsAny<IEvent>(), It.IsAny<CancellationToken>())).Returns(ValueTask.CompletedTask);

        var existingType = CustomFieldEntityType.Create("crm.deal", "Сделка", "deals", CustomFieldEntityIdType.Guid);
        types.Setup(t => t.GetBySpecAsync(It.IsAny<ISpecification<CustomFieldEntityType>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingType);
        types.Setup(t => t.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        defs.Setup(d => d.ExistsAsync(It.IsAny<ISpecification<CustomFieldDefinition>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // поле уже есть

        var addedDefs = new List<CustomFieldDefinition>();
        defs.Setup(d => d.Add(It.IsAny<CustomFieldDefinition>())).Callback<CustomFieldDefinition>(addedDefs.Add);

        var handler = new SyncEntityTypeRegistryCommandHandler(types.Object, defs.Object, bus.Object);
        var cmd = new SyncEntityTypeRegistryCommand([
            new CustomFieldEntityTypeDescriptor("crm.deal", "Сделка", "deals", CustomFieldEntityIdType.Guid,
                PredefinedFields: [new PredefinedFieldDescriptor("industry", "Отрасль", CustomFieldDataType.String)])
        ]);

        await handler.HandleAsync(cmd);

        addedDefs.ShouldBeEmpty();
        types.Verify(t => t.Update(existingType), Times.Once);
    }

    [Fact]
    public async Task BatchGetValues_ReturnsPerEntity_AndDedupsIds()
    {
        var sets = new Mock<IRepository<CustomFieldValueSet, Guid>>();
        var stored = new[]
        {
            CustomFieldValueSet.Create(null, "crm.deal", "a", "{\"x\":1}"),
            CustomFieldValueSet.Create(null, "crm.deal", "b", "{\"y\":2}")
        };
        sets.Setup(s => s.GetAllAsync(It.IsAny<ISpecification<CustomFieldValueSet>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored.ToList());

        var handler = new BatchGetValuesQueryHandler(sets.Object);
        var result = await handler.HandleAsync(new BatchGetValuesQuery("crm.deal", ["a", "b", "a"]));

        result.Count.ShouldBe(2);
        result["a"]["x"].ShouldBe(1L);
        result["b"]["y"].ShouldBe(2L);
    }
}
