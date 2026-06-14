using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Domain.Tests;

/// <summary>Конкретный наследник <see cref="CustomerBase"/> для проверки базового поведения.</summary>
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
