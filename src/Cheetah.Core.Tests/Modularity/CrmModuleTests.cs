using Cheetah.Core.Modularity;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.Tests.Modularity;

public class CrmModuleTests
{
    [Fact]
    public void IsCrmModule_WithValidModuleType_ShouldReturnTrue()
    {
        // Arrange
        var moduleType = typeof(TestModule);

        // Act
        var result = CrmModule.IsCrmModule(moduleType);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsCrmModule_WithAbstractClass_ShouldReturnFalse()
    {
        // Arrange
        var moduleType = typeof(AbstractTestModule);

        // Act
        var result = CrmModule.IsCrmModule(moduleType);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCrmModule_WithInterface_ShouldReturnFalse()
    {
        // Arrange
        var moduleType = typeof(ICrmModule);

        // Act
        var result = CrmModule.IsCrmModule(moduleType);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCrmModule_WithGenericType_ShouldReturnFalse()
    {
        // Arrange
        var moduleType = typeof(GenericTestModule<>);

        // Act
        var result = CrmModule.IsCrmModule(moduleType);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsCrmModule_WithNonModuleClass_ShouldReturnFalse()
    {
        // Arrange
        var moduleType = typeof(NonModuleClass);

        // Act
        var result = CrmModule.IsCrmModule(moduleType);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ConfigureServices_ShouldBeCallable()
    {
        // Arrange
        var module = new TestModule();
        var services = new ServiceCollection();
        var context = new ServiceConfigurationContext(services);

        // Act
        var act = () => module.ConfigureServices(context);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void OnApplicationInitialization_ShouldBeCallable()
    {
        // Arrange
        var module = new TestModule();
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var context = new ApplicationInitializationContext(serviceProvider);

        // Act
        var act = () => module.OnApplicationInitialization(context);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task OnApplicationInitializationAsync_ShouldCallSynchronousVersion()
    {
        // Arrange
        var module = new TrackingTestModule();
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var context = new ApplicationInitializationContext(serviceProvider);

        // Act
        await module.OnApplicationInitializationAsync(context);

        // Assert
        module.OnApplicationInitializationCalled.Should().BeTrue();
    }

    // Test modules
    private class TestModule : CrmModule
    {
    }

    private abstract class AbstractTestModule : CrmModule
    {
    }

    private class GenericTestModule<T> : CrmModule
    {
    }

    private class NonModuleClass
    {
    }

    private class TrackingTestModule : CrmModule
    {
        public bool OnApplicationInitializationCalled { get; private set; }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            OnApplicationInitializationCalled = true;
            base.OnApplicationInitialization(context);
        }
    }
}
