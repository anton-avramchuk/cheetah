using Cheetah.Modules.Customer.Application.Abstractions;
using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Application.Tests;

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

public sealed record TestCreateContactRequest : CreateContactRequestBase;

public sealed record TestUpdateContactRequest : UpdateContactRequestBase;

public sealed record TestContactDto : ContactDtoBase;

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

public sealed class TestContactFactory : IContactFactory<TestContact, TestCreateContactRequest, TestPosition>
{
    public TestContact Create(Guid customerId, TestCreateContactRequest request)
        => TestContact.Create(customerId, request.FullName, request.PositionId, request.Email, request.Phone);
}

public sealed class TestContactProjector : IContactProjector<TestContact, TestContactDto, TestPosition>
{
    public TestContactDto ToDto(TestContact contact) => new()
    {
        Id = contact.Id,
        CustomerId = contact.CustomerId,
        FullName = contact.FullName,
        PositionId = contact.PositionId,
        Email = contact.Email?.Value,
        Phone = contact.Phone?.Value,
        CreatedAt = contact.CreatedAt
    };
}
