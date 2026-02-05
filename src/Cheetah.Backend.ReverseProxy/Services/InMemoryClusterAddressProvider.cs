using Cheetah.OpenApi.Services;
using Yarp.ReverseProxy.Configuration;

namespace Cheetah.Backend.ReverseProxy.Services;

public class InMemoryClusterAddressProvider : IClusterAddressProvider
{
    private readonly IReadOnlyList<RouteConfig> _routes;
    private readonly IReadOnlyList<ClusterConfig> _clusters;

    public InMemoryClusterAddressProvider(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
    {
        _routes = routes;
        _clusters = clusters;
    }

    public IEnumerable<(string Address, string RoutePrefix)> GetClusterAddresses()
    {
        foreach (var route in _routes)
        {
            var clusterId = route.ClusterId;
            var cluster = _clusters.FirstOrDefault(c => c.ClusterId == clusterId);

            if (cluster?.Destinations == null)
                continue;

            var path = route.Match.Path;
            if (string.IsNullOrEmpty(path))
                continue;

            // Преобразуем путь YARP (например, "/vacancy/{**catch-all}") в префикс ("/vacancy/")
            var routePrefix = path.EndsWith("/{**catch-all}", StringComparison.Ordinal)
                ? path.Substring(0, path.Length - "/{**catch-all}".Length)
                : path;

            foreach (var destination in cluster.Destinations)
            {
                if (Uri.TryCreate(destination.Value.Address, UriKind.Absolute, out var _))
                {
                    yield return (destination.Value.Address, routePrefix);
                }
            }
        }
    }
}