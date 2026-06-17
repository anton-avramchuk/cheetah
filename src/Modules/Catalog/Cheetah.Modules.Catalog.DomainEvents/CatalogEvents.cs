using Cheetah.Core.Events;

namespace Cheetah.Modules.Catalog.DomainEvents;

/// <summary>Товар создан в каталоге.</summary>
public record ProductCreatedIntegrationEvent(Guid ProductId, string Sku, string Name, int Type) : EventBase;

/// <summary>Базовые поля товара изменены.</summary>
public record ProductUpdatedIntegrationEvent(Guid ProductId) : EventBase;

/// <summary>Товар деактивирован (снят с продажи).</summary>
public record ProductDeactivatedIntegrationEvent(Guid ProductId) : EventBase;

/// <summary>Товар снова активирован.</summary>
public record ProductActivatedIntegrationEvent(Guid ProductId) : EventBase;

/// <summary>
/// Цена товара в прайс-листе изменена/установлена. Потребитель — Sales Documents (пометить открытые
/// КП «цена устарела») и инвалидация кэша резолва цен.
/// </summary>
public record PriceChangedIntegrationEvent(Guid PriceListId, Guid ProductId, decimal Price, string Currency) : EventBase;
