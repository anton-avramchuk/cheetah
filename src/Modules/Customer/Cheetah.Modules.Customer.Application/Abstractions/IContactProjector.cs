using Cheetah.Modules.Customer.Contracts;
using Cheetah.Modules.Customer.Domain.Entities;

namespace Cheetah.Modules.Customer.Application.Abstractions;

/// <summary>
/// Проекция конкретного контактного лица в конкретный DTO (включая доп. поля наследника и
/// преобразование value objects в строки). Реализуется наследником; используется generic
/// query-handler'ами вместо Mapster, чтобы не требовать скрытой конфигурации VO.
/// </summary>
public interface IContactProjector<in TContact, out TDto, TPosition>
    where TContact : ContactBase<TPosition>
    where TDto : ContactDtoBase
    where TPosition : PositionBase
{
    TDto ToDto(TContact contact);
}
