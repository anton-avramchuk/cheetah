namespace Cheetah.Contracts.Attributes;

/// <summary>
/// HTTP method and semantic type for API route generation
/// </summary>
public enum ApiMethod
{
    /// <summary>GET — returns TResponse (throws on not found)</summary>
    Get,

    /// <summary>GET — returns TResponse? (null on 404)</summary>
    GetOrNotFound,

    /// <summary>GET — returns IReadOnlyList&lt;TResponse&gt;</summary>
    GetCollection,

    /// <summary>GET — returns GridResult&lt;TResponse&gt; from GridRequest</summary>
    GetGrid,

    /// <summary>POST — returns void</summary>
    Post,

    /// <summary>POST — returns TResponse</summary>
    PostWithResult,

    /// <summary>POST — returns Guid (create entity)</summary>
    Create,

    /// <summary>PUT — returns void</summary>
    Update,

    /// <summary>PUT — returns TResponse</summary>
    UpdateWithResult,

    /// <summary>PATCH — returns void</summary>
    Patch,

    /// <summary>DELETE — returns void</summary>
    Delete
}
