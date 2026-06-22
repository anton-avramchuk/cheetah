using Cheetah.Core.Extensions.Collections;

namespace Cheetah.Core.Logging;

public class DefaultInitLoggerFactory : IInitLoggerFactory
{
    private readonly Dictionary<Type, object> _cache = new();

    public virtual IInitLogger<T> Create<T>()
    {
        return (IInitLogger<T>)_cache.GetOrAdd(typeof(T), () => new DefaultInitLogger<T>());
    }
}