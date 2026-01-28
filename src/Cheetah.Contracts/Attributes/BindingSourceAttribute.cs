namespace Cheetah.Contracts.Attributes;

/// <summary>
/// Marker attribute indicating the parameter should be bound from the route
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class FromRouteAttribute : Attribute
{
    public string? Name { get; set; }
}

/// <summary>
/// Marker attribute indicating the parameter should be bound from the request body
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class FromBodyAttribute : Attribute
{
}

/// <summary>
/// Marker attribute indicating the parameter should be bound from the query string
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public class FromQueryAttribute : Attribute
{
    public string? Name { get; set; }
}
