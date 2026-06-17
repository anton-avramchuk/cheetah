using Cheetah.Modules.Catalog.Contracts;
using Cheetah.Modules.Catalog.Domain.Entities;

namespace Cheetah.Modules.Catalog.Application.Abstractions;

/// <summary>
/// Проекция конкретного товара в конкретный DTO (включая доп. поля наследника). Реализуется
/// наследником; используется generic query-handler'ами вместо Mapster, чтобы не требовать скрытой
/// конфигурации маппинга расширенных полей.
/// </summary>
public interface IProductProjector<in TProduct, out TDto>
    where TProduct : ProductBase
    where TDto : ProductDtoBase
{
    TDto ToDto(TProduct product);
}
