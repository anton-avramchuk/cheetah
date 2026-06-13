using System.Text.Json.Serialization;

namespace Cheetah.ApiClientGen;

/// <summary>Root of an <c>apiclients.json</c> file: the list of external clients to generate.</summary>
public sealed class ApiClientsConfig
{
    [JsonPropertyName("clients")]
    public List<ClientConfig> Clients { get; set; } = new();
}

/// <summary>One external API client to generate.</summary>
public sealed class ClientConfig
{
    /// <summary>Logical name; used for the output folder, cache files and the generated class.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    /// <summary><c>rest</c> (OpenAPI → Kiota) for this proof. <c>grpc</c> is reserved for later.</summary>
    [JsonPropertyName("protocol")]
    public string Protocol { get; set; } = "rest";

    /// <summary>URL of the OpenAPI document.</summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;

    /// <summary>Namespace for the generated client.</summary>
    [JsonPropertyName("namespace")]
    public string Namespace { get; set; } = null!;

    /// <summary>Generated client class name. Defaults to <c>{Name}Client</c>.</summary>
    [JsonPropertyName("className")]
    public string? ClassName { get; set; }
}
