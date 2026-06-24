using System;

namespace Cheetah.Mapping.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class MapFromAttribute : Attribute
{
    public Type SourceType { get; }

    public MapFromAttribute(Type sourceType)
    {
        SourceType = sourceType;
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class MapPropertyAttribute : Attribute
{
    public string SourcePropertyName { get; }

    public MapPropertyAttribute(string sourcePropertyName)
    {
        SourcePropertyName = sourcePropertyName;
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class MapIgnoreAttribute : Attribute
{
}

/// <summary>
/// Декларирует генерацию маппера из <c>sourceType</c> в <c>destinationType</c>
/// в той сборке, где размещён сам атрибут (на классе-реестре или на уровне сборки).
/// В отличие от <see cref="MapFromAttribute"/>, маркер не висит на типе-приёмнике, поэтому
/// сборки Contracts/Domain остаются чистыми: знание об обеих сторонах локализовано в выделенной
/// маппинг-сборке (по образцу *.Mapster).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
public class GenerateMapperAttribute : Attribute
{
    public Type SourceType { get; }
    public Type DestinationType { get; }

    /// <summary>
    /// Генерировать ли <c>ProjectTo*</c>-метод для <see cref="System.Linq.IQueryable{T}"/>.
    /// Имеет смысл только для read-проекций (например, Model → ViewModel над запросом к БД).
    /// Для маппингов Request → Command/Query проекция бесполезна — выставьте <c>false</c>,
    /// чтобы генерировался только скалярный <c>MapTo*</c>. По умолчанию <c>true</c>.
    /// </summary>
    public bool GenerateProjection { get; set; } = true;

    public GenerateMapperAttribute(Type sourceType, Type destinationType)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
    }
}

/// <summary>
/// Переопределяет источник для отдельного члена приёмника в маппинге, объявленном через
/// <see cref="GenerateMapperAttribute"/>. Размещается рядом с ним (на классе-реестре или на уровне
/// сборки) и НЕ требует атрибутов на самих типах-участниках — поэтому Contracts/Application/Domain
/// остаются чистыми. Привязка к конкретному маппингу — по типу-приёмнику <c>destinationType</c>.
/// Типичный кейс: <c>DocumentId ← Id</c> (id из роута называется иначе, чем поле команды).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
public class MapMemberAttribute : Attribute
{
    public Type DestinationType { get; }
    public string DestinationMember { get; }
    public string SourceMember { get; }

    public MapMemberAttribute(Type destinationType, string destinationMember, string sourceMember)
    {
        DestinationType = destinationType;
        DestinationMember = destinationMember;
        SourceMember = sourceMember;
    }
}

/// <summary>
/// Указывает, что член приёмника <c>destinationMember</c> в маппинге, объявленном через
/// <see cref="GenerateMapperAttribute"/>, нужно собрать как ВЛОЖЕННЫЙ объект: тот же источник
/// маппится в тип этого члена по обычным правилам (конструктор/совпадение имён). Тип вложенного
/// объекта выводится из типа самого члена. Размещается рядом с <see cref="GenerateMapperAttribute"/>
/// (на классе-реестре или на уровне сборки) — без атрибутов на типах-участниках.
/// Кейс: команда ждёт вложенный DTO (<c>Line: AddLineRequest</c>), а у источника поля плоские.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
public class MapNestedAttribute : Attribute
{
    public Type DestinationType { get; }
    public string DestinationMember { get; }

    public MapNestedAttribute(Type destinationType, string destinationMember)
    {
        DestinationType = destinationType;
        DestinationMember = destinationMember;
    }
}

/// <summary>
/// Задаёт КОНСТАНТНОЕ значение для члена приёмника в маппинге, объявленном через
/// <see cref="GenerateMapperAttribute"/> (источник игнорируется). Значение должно быть
/// константой, допустимой в атрибуте (string/числовой/bool/enum). Размещается рядом
/// с <see cref="GenerateMapperAttribute"/> — без атрибутов на типах-участниках.
/// Кейс: <c>TokenType = "Bearer"</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
public class MapConstantAttribute : Attribute
{
    public Type DestinationType { get; }
    public string DestinationMember { get; }
    public object? Value { get; }

    public MapConstantAttribute(Type destinationType, string destinationMember, object? value)
    {
        DestinationType = destinationType;
        DestinationMember = destinationMember;
        Value = value;
    }
}
