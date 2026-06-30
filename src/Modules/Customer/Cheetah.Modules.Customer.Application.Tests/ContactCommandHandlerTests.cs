using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Customer.Application.Contacts;
using Cheetah.Modules.Customer.Application.Exceptions;
using Cheetah.Modules.Customer.DomainEvents;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Customer.Application.Tests;

public class ContactCommandHandlerTests
{
    private readonly Mock<IRepository<TestContact, Guid>> _repo = new();
    private readonly Mock<IEventBus> _eventBus = new();
    private static readonly Guid CustomerId = Guid.NewGuid();

    [Fact]
    public async Task Add_CreatesSavesAndPublishesAddedEvent()
    {
        var handler = new AddContactCommandHandler<TestContact, TestCreateContactRequest, TestPosition>(
            new TestContactFactory(), _repo.Object, _eventBus.Object);

        var id = await handler.HandleAsync(new AddContactCommand<TestCreateContactRequest>(
            CustomerId, new TestCreateContactRequest { FullName = "John" }));

        id.ShouldNotBe(Guid.Empty);
        _repo.Verify(r => r.Add(It.Is<TestContact>(c => c.FullName == "John" && c.CustomerId == CustomerId)), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(It.Is<IEvent>(e => e is ContactAddedEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_NotFound_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestContact?)null);
        var handler = new UpdateContactCommandHandler<TestContact, TestUpdateContactRequest, TestPosition>(_repo.Object, _eventBus.Object);

        await Should.ThrowAsync<CustomerValidationException>(() =>
            handler.HandleAsync(new UpdateContactCommand<TestUpdateContactRequest>(
                Guid.NewGuid(), new TestUpdateContactRequest { FullName = "X" })).AsTask());
    }

    [Fact]
    public async Task Update_RenamesChangesDetails_AndSaves()
    {
        var contact = TestContact.Create(CustomerId, "John");
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);
        var handler = new UpdateContactCommandHandler<TestContact, TestUpdateContactRequest, TestPosition>(_repo.Object, _eventBus.Object);

        var newPositionId = Guid.NewGuid();
        await handler.HandleAsync(new UpdateContactCommand<TestUpdateContactRequest>(
            contact.Id, new TestUpdateContactRequest { FullName = "Jane", PositionId = newPositionId, Email = "j@x.io" }));

        contact.FullName.ShouldBe("Jane");
        contact.PositionId.ShouldBe(newPositionId);
        contact.Email!.Value.ShouldBe("j@x.io");
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(It.Is<IEvent>(e => e is ContactRenamedEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Remove_SetsRemovedAndPublishes()
    {
        var contact = TestContact.Create(CustomerId, "John");
        _repo.Setup(r => r.GetByIdAsync(contact.Id, It.IsAny<CancellationToken>())).ReturnsAsync(contact);
        var handler = new RemoveContactCommandHandler<TestContact, TestPosition>(_repo.Object, _eventBus.Object);

        await handler.HandleAsync(new RemoveContactCommand(contact.Id));

        contact.RemovedAt.ShouldNotBeNull();
        _eventBus.Verify(b => b.PublishAsync(It.Is<IEvent>(e => e is ContactRemovedEvent), It.IsAny<CancellationToken>()), Times.Once);
    }
}
