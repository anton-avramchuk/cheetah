namespace Cheetah.Core.DependencyInjection;

public interface IObjectAccessor<out T>
{
    T? Value { get; }
}