using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.EntityFramework.DependencyInjection;

public interface IApplicationCommonDbContextRegistrationOptionsBuilder
{
    IServiceCollection Services { get; }
}