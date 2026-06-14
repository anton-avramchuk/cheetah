using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Application.Abstractions;

/// <summary>
/// Фабрика конкретного клиента из запроса на создание. Реализуется наследником — он знает,
/// как сконструировать свою сущность (включая доп. поля) и завести инварианты/события через
/// <c>InitializeCore</c>. Так generic-handler создаёт клиента, не зная конкретного типа.
/// </summary>
public interface ICustomerFactory<out TCustomer, in TCreateRequest>
    where TCustomer : CustomerBase
    where TCreateRequest : CreateCustomerRequestBase
{
    TCustomer Create(TCreateRequest request);
}
