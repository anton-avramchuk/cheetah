using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Extensions.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.Tests.Extensions;

public class ServiceCollectionObjectAccessorExtensionsTests
{
    [Fact]
    public void AddObjectAccessor_WithoutParameter_ShouldAddAccessorToServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var accessor = services.AddObjectAccessor<string>();

        // Assert
        accessor.Should().NotBeNull();
        services.Should().Contain(sd => sd.ServiceType == typeof(ObjectAccessor<string>));
        services.Should().Contain(sd => sd.ServiceType == typeof(IObjectAccessor<string>));
    }

    [Fact]
    public void AddObjectAccessor_WithValue_ShouldAddAccessorWithValue()
    {
        // Arrange
        var services = new ServiceCollection();
        const string expectedValue = "test value";

        // Act
        var accessor = services.AddObjectAccessor(expectedValue);

        // Assert
        accessor.Should().NotBeNull();
        accessor.Value.Should().Be(expectedValue);
    }

    [Fact]
    public void AddObjectAccessor_WithAccessorInstance_ShouldAddToServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var existingAccessor = new ObjectAccessor<int>(42);

        // Act
        var accessor = services.AddObjectAccessor(existingAccessor);

        // Assert
        accessor.Should().BeSameAs(existingAccessor);
        accessor.Value.Should().Be(42);
    }

    [Fact]
    public void AddObjectAccessor_WhenAlreadyRegistered_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddObjectAccessor<string>();

        // Act
        var act = () => services.AddObjectAccessor<string>();

        // Assert
        act.Should().Throw<Exception>()
            .WithMessage("An object accessor is registered before for type: *");
    }

    [Fact]
    public void TryAddObjectAccessor_WhenNotRegistered_ShouldAddAccessor()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var accessor = services.TryAddObjectAccessor<string>();

        // Assert
        accessor.Should().NotBeNull();
        services.Should().Contain(sd => sd.ServiceType == typeof(ObjectAccessor<string>));
    }

    [Fact]
    public void TryAddObjectAccessor_WhenAlreadyRegistered_ShouldReturnExisting()
    {
        // Arrange
        var services = new ServiceCollection();
        var firstAccessor = services.AddObjectAccessor("first");

        // Act
        var secondAccessor = services.TryAddObjectAccessor<string>();

        // Assert
        secondAccessor.Should().BeSameAs(firstAccessor);
        secondAccessor.Value.Should().Be("first");
    }

    [Fact]
    public void GetObjectOrNull_WhenAccessorExists_ShouldReturnValue()
    {
        // Arrange
        var services = new ServiceCollection();
        const string expectedValue = "test value";
        services.AddObjectAccessor(expectedValue);

        // Act
        var result = services.GetObjectOrNull<string>();

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void GetObjectOrNull_WhenAccessorDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.GetObjectOrNull<string>();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetObject_WhenAccessorExists_ShouldReturnValue()
    {
        // Arrange
        var services = new ServiceCollection();
        const string expectedValue = "test value";
        services.AddObjectAccessor(expectedValue);

        // Act
        var result = services.GetObject<string>();

        // Assert
        result.Should().Be(expectedValue);
    }

    [Fact]
    public void GetObject_WhenAccessorDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.GetObject<string>();

        // Assert
        act.Should().Throw<Exception>()
            .WithMessage("Could not find an object of * in services. Be sure that you have used AddObjectAccessor before!");
    }

    [Fact]
    public void AddObjectAccessor_ShouldInsertAtBeginning()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<string>("some service");

        // Act
        services.AddObjectAccessor<int>(42);

        // Assert
        services[0].ServiceType.Should().Be(typeof(IObjectAccessor<int>));
        services[1].ServiceType.Should().Be(typeof(ObjectAccessor<int>));
    }

    [Fact]
    public void AddObjectAccessor_ShouldRegisterBothObjectAccessorAndIObjectAccessor()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddObjectAccessor<string>("test");

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(ObjectAccessor<string>));
        services.Should().Contain(sd => sd.ServiceType == typeof(IObjectAccessor<string>));

        var provider = services.BuildServiceProvider();
        var accessor1 = provider.GetService<ObjectAccessor<string>>();
        var accessor2 = provider.GetService<IObjectAccessor<string>>();

        accessor1.Should().BeSameAs(accessor2);
    }
}
