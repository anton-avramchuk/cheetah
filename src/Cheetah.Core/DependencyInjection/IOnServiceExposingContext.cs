namespace Cheetah.Core.DependencyInjection;

public interface IOnServiceExposingContext
{
    Type ImplementationType { get; }

    List<Type> ExposedTypes { get; }
}