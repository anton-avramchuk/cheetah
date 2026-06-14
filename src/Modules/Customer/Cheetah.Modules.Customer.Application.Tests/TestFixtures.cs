using Cheetah.Modules.Customer.Application.Abstractions;
using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Application.Tests;

public sealed class TestCustomer : CustomerBase
{
    private TestCustomer() { }

    public static TestCustomer Create(string displayName, string? email = null, string? phone = null)
    {
        var customer = new TestCustomer();
        customer.InitializeCore(Guid.NewGuid(), displayName, email, phone);
        return customer;
    }
}

public sealed record TestCreateRequest : CreateCustomerRequestBase;

public sealed record TestUpdateRequest : UpdateCustomerRequestBase;

public sealed record TestCustomerDto : CustomerDtoBase;

public sealed class TestCustomerFactory : ICustomerFactory<TestCustomer, TestCreateRequest>
{
    public TestCustomer Create(TestCreateRequest request)
        => TestCustomer.Create(request.DisplayName, request.Email, request.Phone);
}

public sealed class TestCustomerProjector : ICustomerProjector<TestCustomer, TestCustomerDto>
{
    public TestCustomerDto ToDto(TestCustomer customer) => new()
    {
        Id = customer.Id,
        DisplayName = customer.DisplayName,
        Email = customer.Email?.Value,
        Phone = customer.Phone?.Value,
        Status = customer.Status,
        CreatedAt = customer.CreatedAt
    };
}
