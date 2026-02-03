using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Shouldly;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.Tests.Extensions;

public class ServiceCollectionCommonExtensionsTests
{
    [Fact]
    public void IsAdded_Generic_WhenServiceExists_ShouldReturnTrue()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        // Act
        var result = services.IsAdded<ITestService>();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsAdded_Generic_WhenServiceDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.IsAdded<ITestService>();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsAdded_ByType_WhenServiceExists_ShouldReturnTrue()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        // Act
        var result = services.IsAdded(typeof(ITestService));

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsAdded_ByType_WhenServiceDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.IsAdded(typeof(ITestService));

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void GetSingletonInstanceOrNull_WhenSingletonExists_ShouldReturnInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        var instance = new TestService();
        services.AddSingleton<ITestService>(instance);

        // Act
        var result = services.GetSingletonInstanceOrNull<ITestService>();

        // Assert
        result.ShouldBeSameAs(instance);
    }

    [Fact]
    public void GetSingletonInstanceOrNull_WhenSingletonDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.GetSingletonInstanceOrNull<ITestService>();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetSingletonInstanceOrNull_WhenServiceIsNotSingleton_ShouldReturnNull()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ITestService, TestService>();

        // Act
        var result = services.GetSingletonInstanceOrNull<ITestService>();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetSingletonInstance_WhenSingletonExists_ShouldReturnInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        var instance = new TestService();
        services.AddSingleton<ITestService>(instance);

        // Act
        var result = services.GetSingletonInstance<ITestService>();

        // Assert
        result.ShouldBeSameAs(instance);
    }

    [Fact]
    public void GetSingletonInstance_WhenSingletonDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.GetSingletonInstance<ITestService>();

        // Assert
        Should.Throw<InvalidOperationException>(act).Message.ShouldContain("Could not find singleton service");
    }

    [Fact]
    public void BuildServiceProviderFromFactory_WhenNoFactoryExists_ShouldBuildDefaultProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        // Act
        var provider = services.BuildServiceProviderFromFactory();

        // Assert
        provider.ShouldNotBeNull();
        provider.GetService<ITestService>().ShouldNotBeNull();
    }

    [Fact]
    public void GetServiceProviderOrNull_WhenProviderAccessorExists_ShouldReturnProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        services.AddObjectAccessor<IServiceProvider>(provider);

        // Act
        var result = services.GetServiceProviderOrNull();

        // Assert
        result.ShouldBeSameAs(provider);
    }

    [Fact]
    public void GetServiceProviderOrNull_WhenProviderAccessorDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.GetServiceProviderOrNull();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void GetServiceLazy_Generic_ShouldReturnLazyInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        // Mock ICrmApplication since GetService requires it
        var mockApp = new MockCrmApplication(services.BuildServiceProvider());
        services.Insert(0, ServiceDescriptor.Singleton<ICrmApplication>(mockApp));

        // Act
        var lazy = services.GetServiceLazy<ITestService>();

        // Assert
        lazy.ShouldNotBeNull();
        lazy.IsValueCreated.ShouldBeFalse();
        var value = lazy.Value;
        value.ShouldNotBeNull();
        lazy.IsValueCreated.ShouldBeTrue();
    }

    [Fact]
    public void GetRequiredServiceLazy_Generic_ShouldReturnLazyInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        // Mock ICrmApplication since GetRequiredService requires it
        var mockApp = new MockCrmApplication(services.BuildServiceProvider());
        services.Insert(0, ServiceDescriptor.Singleton<ICrmApplication>(mockApp));

        // Act
        var lazy = services.GetRequiredServiceLazy<ITestService>();

        // Assert
        lazy.ShouldNotBeNull();
        lazy.IsValueCreated.ShouldBeFalse();
        var value = lazy.Value;
        value.ShouldNotBeNull();
        lazy.IsValueCreated.ShouldBeTrue();
    }

    // Test interfaces and classes
    private interface ITestService { }

    private class TestService : ITestService { }

    // Mock implementation of ICrmApplication for testing
    private class MockCrmApplication : ICrmApplication
    {
        public MockCrmApplication(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public Type StartupModuleType => typeof(object);
        public IServiceProvider ServiceProvider { get; }
        public IServiceCollection Services => new ServiceCollection();
        public IReadOnlyList<ICrmModuleDescriptor> Modules => Array.Empty<ICrmModuleDescriptor>();
        public string? ApplicationName => "Test";
        public string InstanceId => Guid.NewGuid().ToString();

        public Task ConfigureServicesAsync() => Task.CompletedTask;
        public void Shutdown()
        {
            
        }

        public Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }

        public void Dispose() { }
    }
}
