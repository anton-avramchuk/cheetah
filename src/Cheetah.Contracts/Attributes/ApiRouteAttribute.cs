namespace Cheetah.Contracts.Attributes;

/// <summary>
/// Marks a request type with its API route and HTTP method for code generation
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ApiRouteAttribute : Attribute
{
    public ApiRouteAttribute(string route, ApiMethod method)
    {
        Route = route;
        Method = method;
    }

    /// <summary>Route template (e.g. "api/candidates" or "api/candidates/{id:guid}")</summary>
    public string Route { get; }

    /// <summary>HTTP method and semantic type</summary>
    public ApiMethod Method { get; }

    /// <summary>Response type for Get/GetOrNotFound/GetGrid/GetCollection methods</summary>
    public Type? ResponseType { get; set; }

    /// <summary>Override generated method name (for disambiguation when two routes map to the same name)</summary>
    public string? MethodName { get; set; }

    /// <summary>Group routes into a separate service (e.g. "Vacancies" → IVacanciesService). If null, uses the name from [GenerateApiClient].</summary>
    public string? ServiceName { get; set; }
}
