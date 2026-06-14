using System.Linq.Expressions;
using Cheetah.Core.Domain.ValueObjects;
using Cheetah.Core.Specification;
using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Domain.Specifications;

/// <summary>Контактные лица указанного клиента (по умолчанию — только не удалённые).</summary>
public sealed class ContactsByCustomerSpecification<TContact> : Specification<TContact>
    where TContact : ContactBase
{
    private readonly Guid _customerId;
    private readonly bool _includeRemoved;

    public ContactsByCustomerSpecification(Guid customerId, bool includeRemoved = false)
    {
        _customerId = customerId;
        _includeRemoved = includeRemoved;
    }

    public override Expression<Func<TContact, bool>> ToExpression()
        => _includeRemoved
            ? c => c.CustomerId == _customerId
            : c => c.CustomerId == _customerId && c.RemovedAt == null;
}

/// <summary>Контактное лицо с указанным email.</summary>
public sealed class ContactByEmailSpecification<TContact> : Specification<TContact>
    where TContact : ContactBase
{
    private readonly Email _email;
    public ContactByEmailSpecification(string email) => _email = Email.Create(email);

    public override Expression<Func<TContact, bool>> ToExpression()
        => c => c.Email == _email;
}
