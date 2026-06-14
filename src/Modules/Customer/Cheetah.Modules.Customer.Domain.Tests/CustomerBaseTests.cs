using Cheetah.Modules.Customer.DomainEvents;
using Cheetah.Modules.Customer.Shared;
using Shouldly;

namespace Cheetah.Modules.Customer.Domain.Tests;

public class CustomerBaseTests
{
    [Fact]
    public void Create_SetsActive_AndRaisesCreatedEvent()
    {
        var customer = TestCustomer.Create("Acme", "info@acme.io", "+12025550123");

        customer.Id.ShouldNotBe(Guid.Empty);
        customer.DisplayName.ShouldBe("Acme");
        customer.Status.ShouldBe(CustomerStatus.Active);
        customer.Email!.Value.ShouldBe("info@acme.io");
        customer.Phone!.Value.ShouldBe("+12025550123");

        var created = customer.DomainEvents.OfType<CustomerCreatedEvent>().ShouldHaveSingleItem();
        created.CustomerId.ShouldBe(customer.Id);
        created.DisplayName.ShouldBe("Acme");
    }

    [Fact]
    public void Create_TrimsDisplayName()
    {
        var customer = TestCustomer.Create("  Acme  ");
        customer.DisplayName.ShouldBe("Acme");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankName_Throws(string name)
    {
        Should.Throw<ArgumentException>(() => TestCustomer.Create(name));
    }

    [Fact]
    public void Create_InvalidEmail_Throws()
    {
        Should.Throw<ArgumentException>(() => TestCustomer.Create("Acme", "not-an-email"));
    }

    [Fact]
    public void Create_NullContacts_LeavesVoNull()
    {
        var customer = TestCustomer.Create("Acme");
        customer.Email.ShouldBeNull();
        customer.Phone.ShouldBeNull();
    }

    [Fact]
    public void Rename_ChangesName_AndRaisesEvent()
    {
        var customer = TestCustomer.Create("Acme");

        customer.Rename("Globex");

        customer.DisplayName.ShouldBe("Globex");
        customer.DomainEvents.OfType<CustomerRenamedEvent>().ShouldHaveSingleItem()
            .DisplayName.ShouldBe("Globex");
    }

    [Fact]
    public void ChangeContacts_UpdatesVo_AndRaisesEvent()
    {
        var customer = TestCustomer.Create("Acme");

        customer.ChangeContacts("hello@acme.io", "+12025550199");

        customer.Email!.Value.ShouldBe("hello@acme.io");
        customer.Phone!.Value.ShouldBe("+12025550199");
        var changed = customer.DomainEvents.OfType<CustomerContactsChangedEvent>().ShouldHaveSingleItem();
        changed.Email.ShouldBe("hello@acme.io");
        changed.Phone.ShouldBe("+12025550199");
    }

    [Fact]
    public void Archive_SetsStatus_RemovedAt_AndEvent()
    {
        var customer = TestCustomer.Create("Acme");

        customer.Archive();

        customer.Status.ShouldBe(CustomerStatus.Archived);
        customer.RemovedAt.ShouldNotBeNull();
        customer.DomainEvents.OfType<CustomerArchivedEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Archive_Twice_IsIdempotent()
    {
        var customer = TestCustomer.Create("Acme");

        customer.Archive();
        customer.Archive();

        customer.DomainEvents.OfType<CustomerArchivedEvent>().Count().ShouldBe(1);
    }

    [Fact]
    public void AssignOwner_SetsOwner_AndRaisesEvent()
    {
        var customer = TestCustomer.Create("Acme");
        var ownerId = Guid.NewGuid();

        customer.AssignOwner(ownerId);

        customer.OwnerId.ShouldBe(ownerId);
        customer.DomainEvents.OfType<CustomerOwnerChangedEvent>().ShouldHaveSingleItem()
            .OwnerId.ShouldBe(ownerId);
    }

    [Fact]
    public void AssignOwner_SameValue_IsIdempotent()
    {
        var customer = TestCustomer.Create("Acme");
        var ownerId = Guid.NewGuid();

        customer.AssignOwner(ownerId);
        customer.AssignOwner(ownerId);

        customer.DomainEvents.OfType<CustomerOwnerChangedEvent>().Count().ShouldBe(1);
    }
}
