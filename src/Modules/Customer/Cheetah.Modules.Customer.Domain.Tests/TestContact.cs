using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Domain.Tests;

/// <summary>Конкретный наследник <see cref="ContactBase{TPosition}"/> для проверки базового поведения.</summary>
public sealed class TestContact : ContactBase<TestPosition>
{
    private TestContact() { }

    public static TestContact Create(
        Guid customerId, string fullName, Guid? positionId = null, string? email = null, string? phone = null)
    {
        var contact = new TestContact();
        contact.InitializeCore(Guid.NewGuid(), customerId, fullName, positionId, email, phone);
        return contact;
    }
}

/// <summary>Конкретный наследник <see cref="PositionBase"/> для тестов.</summary>
public sealed class TestPosition : PositionBase
{
    private TestPosition() { }

    public static TestPosition Create(string name)
    {
        var position = new TestPosition();
        position.InitializeCore(Guid.NewGuid(), name);
        return position;
    }
}
