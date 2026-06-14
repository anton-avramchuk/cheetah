using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Application.Abstractions;

/// <summary>
/// Проекция конкретного клиента в конкретный DTO (включая доп. поля наследника и
/// преобразование value objects в строки). Реализуется наследником; используется
/// generic query-handler'ами вместо Mapster, чтобы не требовать скрытой конфигурации VO.
/// </summary>
public interface ICustomerProjector<in TCustomer, out TDto>
    where TCustomer : CustomerBase
    where TDto : CustomerDtoBase
{
    TDto ToDto(TCustomer customer);
}
