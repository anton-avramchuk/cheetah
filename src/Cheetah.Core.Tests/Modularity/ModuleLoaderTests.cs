using Cheetah.Core.Modularity;
using Shouldly;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.Tests.Modularity;

public class ModuleLoaderTests
{
    [Fact]
    public void LoadModules_ShouldReturnArrayOfModuleDescriptors()
    {
        // Arrange
        var loader = new ModuleLoader();
        var services = new ServiceCollection();

        // Act
        var modules = loader.LoadModules(services, typeof(TestModule));

        // Assert
        modules.ShouldNotBeNull();
        modules.ShouldBeOfType<ICrmModuleDescriptor[]>();
    }

    [Fact]
    public void LoadModules_ShouldRegisterModulesInServiceCollection()
    {
        // Arrange
        var loader = new ModuleLoader();
        var services = new ServiceCollection();
        ModuleInitializer.AddModule<TestModule>();

        // Act
        var modules = loader.LoadModules(services, typeof(TestModule));

        // Assert
        services.ShouldContain(sd =>
            sd.ServiceType == typeof(TestModule) &&
            sd.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void LoadModules_ShouldCreateModuleInstances()
    {
        // Arrange
        var loader = new ModuleLoader();
        var services = new ServiceCollection();
        ModuleInitializer.AddModule<TestModule>();

        // Act
        var modules = loader.LoadModules(services, typeof(TestModule));

        // Assert
        foreach (var m in modules)
            m.Instance.ShouldNotBeNull();
    }

    // Test module
    private class TestModule : CrmModule
    {
    }
}
