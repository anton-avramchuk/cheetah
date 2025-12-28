using Cheetah.Core.Modularity;
using FluentAssertions;
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
        modules.Should().NotBeNull();
        modules.Should().BeOfType<ICrmModuleDescriptor[]>();
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
        services.Should().Contain(sd =>
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
        modules.Should().AllSatisfy(m => m.Instance.Should().NotBeNull());
    }

    // Test module
    private class TestModule : CrmModule
    {
    }
}
