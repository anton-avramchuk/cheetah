using Cheetah.Core.Exceptions;

namespace Cheetah.Core;

public sealed class NullDisposable : IDisposable
{
    public static NullDisposable Instance { get; } = new NullDisposable();

    private bool _disposed;

    private NullDisposable()
    {

    }

    public void Dispose()
    {
        if (_disposed)
        {
            _disposed = true;
        }
        else
        {
            throw new DoubleDisposedException();
        }
    }
}