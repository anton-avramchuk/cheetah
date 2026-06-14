using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Domain.Tests;

/// <summary>Конкретный наследник <see cref="ContactBase"/> для проверки базового поведения.</summary>
public sealed class TestContact : ContactBase
{
    private TestContact() { }

    public static TestContact Create(
        Guid customerId, string fullName, string? position = null, string? email = null, string? phone = null)
    {
        var contact = new TestContact();
        contact.InitializeCore(Guid.NewGuid(), customerId, fullName, position, email, phone);
        return contact;
    }
}
