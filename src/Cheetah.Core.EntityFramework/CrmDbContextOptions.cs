using Cheetah.Core.Exceptions;

namespace Cheetah.Core.EntityFramework;

public class CrmDbContextOptions
{

    public void Configure(Action<CrmDbContextConfigurationContext> action)
    {
        DefaultConfigureAction = action;
    }

    public void Configure<TDbContext>(Action<ApplicationDbContextConfigurationContext<TDbContext>> action)
        where TDbContext : CrmDbContext<TDbContext>
    {

        ConfigureActions[typeof(TDbContext)] = action;
    }

    internal Action<CrmDbContextConfigurationContext>? DefaultConfigureAction { get; set; }

    internal Dictionary<Type, object> ConfigureActions { get; } = new();

    internal Dictionary<Type, Type> DbContextReplacements { get; } = new();


    internal Type GetReplacedTypeOrSelf(Type dbContextType)
    {
        var replacementType = dbContextType;
        while (true)
        {
            if (DbContextReplacements.TryGetValue(replacementType, out var foundType))
            {
                if (foundType == dbContextType)
                {
                    throw new CrmException(
                        "Circular DbContext replacement found for " +
                        dbContextType.AssemblyQualifiedName
                    );
                }

                replacementType = foundType;
            }
            else
            {
                return replacementType;
            }
        }
    }
}