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
