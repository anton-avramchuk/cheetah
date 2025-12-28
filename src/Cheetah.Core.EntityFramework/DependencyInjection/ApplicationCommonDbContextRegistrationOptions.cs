using Cheetah.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.EntityFramework.DependencyInjection;

public abstract class ApplicationCommonDbContextRegistrationOptions :
    IApplicationCommonDbContextRegistrationOptionsBuilder, IApplicationDbContextRegistrationOptionsBuilder
{
    public IServiceCollection Services { get; }
    public Type OriginalDbContextType { get; }

    public Dictionary<Type, Type?> ReplacedDbContextTypes { get; } = new();

    protected ApplicationCommonDbContextRegistrationOptions(Type originalDbContextType, IServiceCollection services)
    {
        OriginalDbContextType = originalDbContextType ?? throw new ArgumentNullException(nameof(originalDbContextType));
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public IApplicationCommonDbContextRegistrationOptionsBuilder ReplaceDbContext(Type otherDbContextType,
        Type? targetDbContextType = null)
    {
        if (!otherDbContextType.IsAssignableFrom(OriginalDbContextType))
        {
            throw new CrmException(
                $"{OriginalDbContextType.AssemblyQualifiedName} should inherit/implement {otherDbContextType.AssemblyQualifiedName}!");
        }

        ReplacedDbContextTypes[otherDbContextType] = targetDbContextType;

        return this;
    }
}