using System.Linq.Expressions;
using Cheetah.Core.Domain.ValueObjects;
using Cheetah.Core.Specification;
using Cheetah.Modules.Customer.Domain.Entities;
using Cheetah.Modules.Customer.Shared;

namespace Cheetah.Modules.Customer.Domain.Specifications;

/// <summary>Клиент с указанным email (сравнение по value-converted колонке).</summary>
public sealed class CustomerByEmailSpecification<TCustomer> : Specification<TCustomer>
    where TCustomer : CustomerBase
{
    private readonly Email _email;
    public CustomerByEmailSpecification(string email) => _email = Email.Create(email);

    public override Expression<Func<TCustomer, bool>> ToExpression()
        => c => c.Email == _email;
}

/// <summary>Клиенты по набору идентификаторов.</summary>
public sealed class CustomerByIdsSpecification<TCustomer> : Specification<TCustomer>
    where TCustomer : CustomerBase
{
    private readonly IReadOnlyCollection<Guid> _ids;
    public CustomerByIdsSpecification(IReadOnlyCollection<Guid> ids) => _ids = ids;

    public override Expression<Func<TCustomer, bool>> ToExpression()
        => c => _ids.Contains(c.Id);
}

/// <summary>Только активные клиенты.</summary>
public sealed class ActiveCustomersSpecification<TCustomer> : Specification<TCustomer>
    where TCustomer : CustomerBase
{
    public override Expression<Func<TCustomer, bool>> ToExpression()
        => c => c.Status == CustomerStatus.Active;
}

/// <summary>Клиенты, закреплённые за указанным ответственным (пользователем Identity).</summary>
public sealed class CustomersByOwnerSpecification<TCustomer> : Specification<TCustomer>
    where TCustomer : CustomerBase
{
    private readonly Guid _ownerId;
    public CustomersByOwnerSpecification(Guid ownerId) => _ownerId = ownerId;

    public override Expression<Func<TCustomer, bool>> ToExpression()
        => c => c.OwnerId == _ownerId;
}
