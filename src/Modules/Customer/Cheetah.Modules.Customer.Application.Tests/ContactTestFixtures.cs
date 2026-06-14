using Cheetah.Modules.Customer.Application.Abstractions;
using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Application.Tests;

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

public sealed record TestCreateContactRequest : CreateContactRequestBase;

public sealed record TestUpdateContactRequest : UpdateContactRequestBase;

public sealed record TestContactDto : ContactDtoBase;

public sealed class TestContactFactory : IContactFactory<TestContact, TestCreateContactRequest>
{
    public TestContact Create(Guid customerId, TestCreateContactRequest request)
        => TestContact.Create(customerId, request.FullName, request.Position, request.Email, request.Phone);
}

public sealed class TestContactProjector : IContactProjector<TestContact, TestContactDto>
{
    public TestContactDto ToDto(TestContact contact) => new()
    {
        Id = contact.Id,
        CustomerId = contact.CustomerId,
        FullName = contact.FullName,
        Position = contact.Position,
        Email = contact.Email?.Value,
        Phone = contact.Phone?.Value,
        CreatedAt = contact.CreatedAt
    };
}
