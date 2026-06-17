namespace Cheetah.Modules.Catalog.Shared;

/// <summary>Вид номенклатуры.</summary>
public enum ProductType
{
    Goods = 0,
    Service = 1,
    DigitalGoods = 2,
    Bundle = 3
}

/// <summary>Единица измерения товара/услуги.</summary>
public enum UnitOfMeasure
{
    Piece = 0,
    Hour = 1,
    Kilogram = 2,
    Liter = 3,
    Meter = 4,
    Pack = 5
}
