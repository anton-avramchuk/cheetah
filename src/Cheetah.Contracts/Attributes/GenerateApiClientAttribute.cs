namespace Cheetah.Contracts.Attributes;

/// <summary>
/// Triggers API client source generation for the annotated module class
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class GenerateApiClientAttribute : Attribute
{
    public GenerateApiClientAttribute(string serviceName)
    {
        ServiceName = serviceName;
    }

    /// <summary>Service name used for I{Name}Service / {Name}Service</summary>
    public string ServiceName { get; }
}
