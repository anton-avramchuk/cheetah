using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Customer.Application.Customers;
using Cheetah.Modules.Customer.Application.Exceptions;
using Cheetah.Modules.Customer.DomainEvents;
using Cheetah.Modules.Customer.Shared;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Customer.Application.Tests;

public class CustomerCommandHandlerTests
{
    private readonly Mock<IRepository<TestCustomer, Guid>> _repo = new();
    private readonly Mock<IEventBus> _eventBus = new();

    [Fact]
    public async Task Create_AddsSavesAndPublishesCreatedEvent()
    {
        var handler = new CreateCustomerCommandHandler<TestCustomer, TestCreateRequest>(
            new TestCustomerFactory(), _repo.Object, _eventBus.Object);

        var id = await handler.HandleAsync(
            new CreateCustomerCommand<TestCreateRequest>(new TestCreateRequest { DisplayName = "Acme" }));

        id.ShouldNotBe(Guid.Empty);
        _repo.Verify(r => r.Add(It.Is<TestCustomer>(c => c.DisplayName == "Acme")), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(It.Is<IEvent>(e => e is CustomerCreatedEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_NotFound_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestCustomer?)null);
        var handler = new UpdateCustomerCommandHandler<TestCustomer, TestUpdateRequest>(_repo.Object, _eventBus.Object);

        await Should.ThrowAsync<CustomerValidationException>(() =>
            handler.HandleAsync(new UpdateCustomerCommand<TestUpdateRequest>(
                Guid.NewGuid(), new TestUpdateRequest { DisplayName = "X" })).AsTask());
    }

    [Fact]
    public async Task Update_RenamesChangesContacts_AndSaves()
    {
        var customer = TestCustomer.Create("Acme");
        _repo.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        var handler = new UpdateCustomerCommandHandler<TestCustomer, TestUpdateRequest>(_repo.Object, _eventBus.Object);

        await handler.HandleAsync(new UpdateCustomerCommand<TestUpdateRequest>(
            customer.Id, new TestUpdateRequest { DisplayName = "Globex", Email = "g@x.io" }));

        customer.DisplayName.ShouldBe("Globex");
        customer.Email!.Value.ShouldBe("g@x.io");
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(It.Is<IEvent>(e => e is CustomerRenamedEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_AssignsOwner_AndPublishesOwnerChanged()
    {
        var customer = TestCustomer.Create("Acme");
        _repo.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        var handler = new UpdateCustomerCommandHandler<TestCustomer, TestUpdateRequest>(_repo.Object, _eventBus.Object);
        var ownerId = Guid.NewGuid();

        await handler.HandleAsync(new UpdateCustomerCommand<TestUpdateRequest>(
            customer.Id, new TestUpdateRequest { DisplayName = "Acme", OwnerId = ownerId }));

        customer.OwnerId.ShouldBe(ownerId);
        _eventBus.Verify(b => b.PublishAsync(It.Is<IEvent>(e => e is CustomerOwnerChangedEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Archive_SetsStatusAndPublishes()
    {
        var customer = TestCustomer.Create("Acme");
        _repo.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);
        var handler = new ArchiveCustomerCommandHandler<TestCustomer>(_repo.Object, _eventBus.Object);

        await handler.HandleAsync(new ArchiveCustomerCommand(customer.Id));

        customer.Status.ShouldBe(CustomerStatus.Archived);
        _eventBus.Verify(b => b.PublishAsync(It.Is<IEvent>(e => e is CustomerArchivedEvent), It.IsAny<CancellationToken>()), Times.Once);
    }
}
