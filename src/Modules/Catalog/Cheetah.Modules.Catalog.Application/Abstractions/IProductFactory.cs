using Cheetah.Modules.Catalog.Contracts;
using Cheetah.Modules.Catalog.Domain.Entities;

namespace Cheetah.Modules.Catalog.Application.Abstractions;

/// <summary>
/// Фабрика конкретного товара из запроса на создание. Реализуется наследником — он знает, как
/// сконструировать свою сущность (включая доп. поля) и завести инварианты/события через
/// <c>InitializeCore</c>. Так generic-handler создаёт товар, не зная конкретного типа.
/// </summary>
public interface IProductFactory<out TProduct, in TCreateRequest>
    where TProduct : ProductBase
    where TCreateRequest : CreateProductRequestBase
{
    TProduct Create(TCreateRequest request);
}
