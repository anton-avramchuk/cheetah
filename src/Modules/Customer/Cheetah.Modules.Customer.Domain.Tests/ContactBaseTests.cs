using Cheetah.Modules.Customer.DomainEvents;
using Shouldly;

namespace Cheetah.Modules.Customer.Domain.Tests;

public class ContactBaseTests
{
    private static readonly Guid CustomerId = Guid.NewGuid();
    private static readonly Guid PositionId = Guid.NewGuid();

    [Fact]
    public void Create_SetsFields_AndRaisesAddedEvent()
    {
        var contact = TestContact.Create(CustomerId, "John Doe", PositionId, "john@acme.io", "+12025550123");

        contact.Id.ShouldNotBe(Guid.Empty);
        contact.CustomerId.ShouldBe(CustomerId);
        contact.FullName.ShouldBe("John Doe");
        contact.PositionId.ShouldBe(PositionId);
        contact.Email!.Value.ShouldBe("john@acme.io");
        contact.Phone!.Value.ShouldBe("+12025550123");

        var added = contact.DomainEvents.OfType<ContactAddedEvent>().ShouldHaveSingleItem();
        added.ContactId.ShouldBe(contact.Id);
        added.CustomerId.ShouldBe(CustomerId);
        added.FullName.ShouldBe("John Doe");
    }

    [Fact]
    public void Create_EmptyCustomerId_Throws()
    {
        Should.Throw<ArgumentException>(() => TestContact.Create(Guid.Empty, "John"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankName_Throws(string name)
    {
        Should.Throw<ArgumentException>(() => TestContact.Create(CustomerId, name));
    }

    [Fact]
    public void Create_TrimsName_AndKeepsNullPosition()
    {
        var contact = TestContact.Create(CustomerId, "  John  ");
        contact.FullName.ShouldBe("John");
        contact.PositionId.ShouldBeNull();
    }

    [Fact]
    public void Rename_ChangesName_AndRaisesEvent()
    {
        var contact = TestContact.Create(CustomerId, "John");

        contact.Rename("Jane");

        contact.FullName.ShouldBe("Jane");
        contact.DomainEvents.OfType<ContactRenamedEvent>().ShouldHaveSingleItem()
            .FullName.ShouldBe("Jane");
    }

    [Fact]
    public void ChangeContacts_UpdatesVo_AndRaisesEvent()
    {
        var contact = TestContact.Create(CustomerId, "John");

        contact.ChangeContacts("j@acme.io", "+12025550199");

        contact.Email!.Value.ShouldBe("j@acme.io");
        contact.Phone!.Value.ShouldBe("+12025550199");
        var changed = contact.DomainEvents.OfType<ContactContactsChangedEvent>().ShouldHaveSingleItem();
        changed.Email.ShouldBe("j@acme.io");
        changed.Phone.ShouldBe("+12025550199");
    }

    /// <summary>
    /// Мессенджеры — такие же значимые типы, как почта и телефон: приводятся к канону при записи.
    /// Ник копируют из профиля вместе с адресом страницы, поэтому «t.me/john» и «@John» — это одно
    /// и то же значение.
    /// </summary>
    [Fact]
    public void ChangeContacts_CanonicalizesMessengers()
    {
        var contact = TestContact.Create(CustomerId, "John");

        contact.ChangeContacts("j@acme.io", "+12025550199", "https://t.me/JohnDoe", "wa.me/+12025550199");

        contact.Telegram!.Value.ShouldBe("johndoe");
        contact.WhatsApp!.Value.ShouldBe("+12025550199");
    }

    [Fact]
    public void Create_TakesMessengers()
    {
        var contact = TestContact.Create(CustomerId, "John", telegram: "@john_doe", whatsApp: "+12025550199");

        contact.Telegram!.Value.ShouldBe("john_doe");
        contact.WhatsApp!.Value.ShouldBe("+12025550199");
    }

    /// <summary>
    /// Не присланный мессенджер очищается, а не остаётся прежним: способы связи правят формой, где
    /// все поля видны сразу, и «пусто» там значит «убрали».
    /// </summary>
    [Fact]
    public void ChangeContacts_WithoutMessengers_ClearsThem()
    {
        var contact = TestContact.Create(CustomerId, "John", telegram: "@john_doe", whatsApp: "+12025550199");

        contact.ChangeContacts("j@acme.io", "+12025550199");

        contact.Telegram.ShouldBeNull();
        contact.WhatsApp.ShouldBeNull();
    }

    /// <summary>Негодное значение — исключение: мусор не должен доехать до кнопки «написать».</summary>
    [Fact]
    public void ChangeContacts_RejectsGarbageMessengers()
    {
        var contact = TestContact.Create(CustomerId, "John");

        Should.Throw<ArgumentException>(() => contact.ChangeContacts(null, null, "@a", null));
        Should.Throw<ArgumentException>(() => contact.ChangeContacts(null, null, null, "написать в ватсап"));
    }

    [Fact]
    public void Remove_SetsRemovedAt_AndRaisesEvent()
    {
        var contact = TestContact.Create(CustomerId, "John");

        contact.Remove();

        contact.RemovedAt.ShouldNotBeNull();
        contact.DomainEvents.OfType<ContactRemovedEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Remove_Twice_IsIdempotent()
    {
        var contact = TestContact.Create(CustomerId, "John");

        contact.Remove();
        contact.Remove();

        contact.DomainEvents.OfType<ContactRemovedEvent>().Count().ShouldBe(1);
    }
}
