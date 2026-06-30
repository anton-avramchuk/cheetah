using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Application.Abstractions;

/// <summary>
/// Фабрика конкретного контактного лица из запроса на добавление. Реализуется наследником —
/// он знает, как сконструировать свою сущность (включая доп. поля) и завести инварианты/события
/// через <c>InitializeCore</c>. <c>customerId</c> приходит из маршрута, а не из тела запроса.
/// </summary>
public interface IContactFactory<out TContact, in TCreateRequest, TPosition>
    where TContact : ContactBase<TPosition>
    where TCreateRequest : CreateContactRequestBase
    where TPosition : PositionBase
{
    TContact Create(Guid customerId, TCreateRequest request);
}
