using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using FluentAssertions;
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
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAdded_Generic_WhenServiceDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.IsAdded<ITestService>();

        // Assert
        result.Should().BeFalse();
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
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAdded_ByType_WhenServiceDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.IsAdded(typeof(ITestService));

        // Assert
        result.Should().BeFalse();
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
        result.Should().BeSameAs(instance);
    }

    [Fact]
    public void GetSingletonInstanceOrNull_WhenSingletonDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.GetSingletonInstanceOrNull<ITestService>();

        // Assert
        result.Should().BeNull();
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
        result.Should().BeNull();
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
        result.Should().BeSameAs(instance);
    }

    [Fact]
    public void GetSingletonInstance_WhenSingletonDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.GetSingletonInstance<ITestService>();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Could not find singleton service: *");
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
        provider.Should().NotBeNull();
        provider.GetService<ITestService>().Should().NotBeNull();
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
        result.Should().BeSameAs(provider);
    }

    [Fact]
    public void GetServiceProviderOrNull_WhenProviderAccessorDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.GetServiceProviderOrNull();

        // Assert
        result.Should().BeNull();
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
        lazy.Should().NotBeNull();
        lazy.IsValueCreated.Should().BeFalse();
        var value = lazy.Value;
        value.Should().NotBeNull();
        lazy.IsValueCreated.Should().BeTrue();
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
        lazy.Should().NotBeNull();
        lazy.IsValueCreated.Should().BeFalse();
        var value = lazy.Value;
        value.Should().NotBeNull();
        lazy.IsValueCreated.Should().BeTrue();
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

        public void Dispose() { }
    }
}
