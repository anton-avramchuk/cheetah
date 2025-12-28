using Cheetah.Core.Modularity;
using FluentAssertions;

namespace Cheetah.Core.Tests.Modularity;

public class CrmModuleDescriptorTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateDescriptor()
    {
        // Arrange
        var module = new TestModule();
        var moduleType = typeof(TestModule);

        // Act
        var descriptor = new CrmModuleDescriptor(moduleType, module);

        // Assert
        descriptor.Should().NotBeNull();
        descriptor.Type.Should().Be(moduleType);
        descriptor.Instance.Should().BeSameAs(module);
        descriptor.Assembly.Should().BeSameAs(moduleType.Assembly);
        descriptor.Dependencies.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithMismatchedInstanceType_ShouldThrowArgumentException()
    {
        // Arrange
        var module = new TestModule();
        var differentModuleType = typeof(AnotherTestModule);

        // Act
        var act = () => new CrmModuleDescriptor(differentModuleType, module);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Given module instance * is not an instance of given module type: *");
    }

    [Fact]
    public void AddDependency_ShouldAddDependencyToList()
    {
        // Arrange
        var module = new TestModule();
        var descriptor = new CrmModuleDescriptor(typeof(TestModule), module);

        var dependencyModule = new AnotherTestModule();
        var dependencyDescriptor = new CrmModuleDescriptor(typeof(AnotherTestModule), dependencyModule);

        // Act
        descriptor.AddDependency(dependencyDescriptor);

        // Assert
        descriptor.Dependencies.Should().ContainSingle();
        descriptor.Dependencies.Should().Contain(dependencyDescriptor);
    }

    [Fact]
    public void AddDependency_WithSameDependencyTwice_ShouldNotDuplicate()
    {
        // Arrange
        var module = new TestModule();
        var descriptor = new CrmModuleDescriptor(typeof(TestModule), module);

        var dependencyModule = new AnotherTestModule();
        var dependencyDescriptor = new CrmModuleDescriptor(typeof(AnotherTestModule), dependencyModule);

        // Act
        descriptor.AddDependency(dependencyDescriptor);
        descriptor.AddDependency(dependencyDescriptor);

        // Assert
        descriptor.Dependencies.Should().ContainSingle();
    }

    [Fact]
    public void AddDependency_WithMultipleDependencies_ShouldAddAll()
    {
        // Arrange
        var module = new TestModule();
        var descriptor = new CrmModuleDescriptor(typeof(TestModule), module);

        var dependency1 = new CrmModuleDescriptor(typeof(AnotherTestModule), new AnotherTestModule());
        var dependency2 = new CrmModuleDescriptor(typeof(ThirdTestModule), new ThirdTestModule());

        // Act
        descriptor.AddDependency(dependency1);
        descriptor.AddDependency(dependency2);

        // Assert
        descriptor.Dependencies.Should().HaveCount(2);
        descriptor.Dependencies.Should().Contain(dependency1);
        descriptor.Dependencies.Should().Contain(dependency2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var module = new TestModule();
        var descriptor = new CrmModuleDescriptor(typeof(TestModule), module);

        // Act
        var result = descriptor.ToString();

        // Assert
        result.Should().Contain("CrmModuleDescriptor");
        result.Should().Contain(typeof(TestModule).FullName!);
    }

    [Fact]
    public void Dependencies_ShouldBeReadOnly()
    {
        // Arrange
        var module = new TestModule();
        var descriptor = new CrmModuleDescriptor(typeof(TestModule), module);

        // Act & Assert
        descriptor.Dependencies.Should().BeAssignableTo<IReadOnlyList<ICrmModuleDescriptor>>();
    }

    [Fact]
    public void AllAssemblies_ShouldIncludeModuleAssembly()
    {
        // Arrange
        var module = new TestModule();
        var descriptor = new CrmModuleDescriptor(typeof(TestModule), module);

        // Act & Assert
        descriptor.AllAssemblies.Should().NotBeEmpty();
        descriptor.AllAssemblies.Should().Contain(descriptor.Assembly);
    }

    // Test modules
    private class TestModule : CrmModule
    {
    }

    private class AnotherTestModule : CrmModule
    {
    }

    private class ThirdTestModule : CrmModule
    {
    }

    private class NonModuleClass
    {
    }
}
