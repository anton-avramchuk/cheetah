using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Extensions.DependencyInjection;
using Shouldly;
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
        accessor.ShouldNotBeNull();
        services.ShouldContain(sd => sd.ServiceType == typeof(ObjectAccessor<string>));
        services.ShouldContain(sd => sd.ServiceType == typeof(IObjectAccessor<string>));
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
        accessor.ShouldNotBeNull();
        accessor.Value.ShouldBe(expectedValue);
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
        accessor.ShouldBeSameAs(existingAccessor);
        accessor.Value.ShouldBe(42);
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
        Should.Throw<Exception>(act).Message.ShouldContain("An object accessor is registered before for type");
    }

    [Fact]
    public void TryAddObjectAccessor_WhenNotRegistered_ShouldAddAccessor()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var accessor = services.TryAddObjectAccessor<string>();

        // Assert
        accessor.ShouldNotBeNull();
        services.ShouldContain(sd => sd.ServiceType == typeof(ObjectAccessor<string>));
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
        secondAccessor.ShouldBeSameAs(firstAccessor);
        secondAccessor.Value.ShouldBe("first");
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
        result.ShouldBe(expectedValue);
    }

    [Fact]
    public void GetObjectOrNull_WhenAccessorDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.GetObjectOrNull<string>();

        // Assert
        result.ShouldBeNull();
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
        result.ShouldBe(expectedValue);
    }

    [Fact]
    public void GetObject_WhenAccessorDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.GetObject<string>();

        // Assert
        Should.Throw<Exception>(act).Message.ShouldContain("Could not find an object of");
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
        services[0].ServiceType.ShouldBe(typeof(IObjectAccessor<int>));
        services[1].ServiceType.ShouldBe(typeof(ObjectAccessor<int>));
    }

    [Fact]
    public void AddObjectAccessor_ShouldRegisterBothObjectAccessorAndIObjectAccessor()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddObjectAccessor<string>("test");

        // Assert
        services.ShouldContain(sd => sd.ServiceType == typeof(ObjectAccessor<string>));
        services.ShouldContain(sd => sd.ServiceType == typeof(IObjectAccessor<string>));

        var provider = services.BuildServiceProvider();
        var accessor1 = provider.GetService<ObjectAccessor<string>>();
        var accessor2 = provider.GetService<IObjectAccessor<string>>();

        accessor1.ShouldBeSameAs(accessor2);
    }
}
