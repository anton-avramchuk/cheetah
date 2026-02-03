using Cheetah.Core.Modularity;
using Shouldly;

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
        descriptor.ShouldNotBeNull();
        descriptor.Type.ShouldBe(moduleType);
        descriptor.Instance.ShouldBeSameAs(module);
        descriptor.Assembly.ShouldBeSameAs(moduleType.Assembly);
        descriptor.Dependencies.ShouldBeEmpty();
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
        Should.Throw<ArgumentException>(act).Message.ShouldContain("is not an instance of given module type");
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
        descriptor.Dependencies.Count.ShouldBe(1);
        descriptor.Dependencies.ShouldContain(dependencyDescriptor);
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
        descriptor.Dependencies.Count.ShouldBe(1);
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
        descriptor.Dependencies.Count.ShouldBe(2);
        descriptor.Dependencies.ShouldContain(dependency1);
        descriptor.Dependencies.ShouldContain(dependency2);
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
        result.ShouldContain("CrmModuleDescriptor");
        result.ShouldContain(typeof(TestModule).FullName!);
    }

    [Fact]
    public void Dependencies_ShouldBeReadOnly()
    {
        // Arrange
        var module = new TestModule();
        var descriptor = new CrmModuleDescriptor(typeof(TestModule), module);

        // Act & Assert
        descriptor.Dependencies.ShouldBeAssignableTo<IReadOnlyList<ICrmModuleDescriptor>>();
    }

    [Fact]
    public void AllAssemblies_ShouldIncludeModuleAssembly()
    {
        // Arrange
        var module = new TestModule();
        var descriptor = new CrmModuleDescriptor(typeof(TestModule), module);

        // Act & Assert
        descriptor.AllAssemblies.ShouldNotBeEmpty();
        descriptor.AllAssemblies.ShouldContain(descriptor.Assembly);
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
