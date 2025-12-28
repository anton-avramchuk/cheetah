using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.EntityFramework.DependencyInjection;

public class ApplicationDbContextRegistrationOptions(Type originalDbContextType, IServiceCollection services)
    : ApplicationCommonDbContextRegistrationOptions(originalDbContextType, services),
        IApplicationCommonDbContextRegistrationOptionsBuilder;