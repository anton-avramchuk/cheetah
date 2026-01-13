namespace Cheetah.Analyzers.Tests;

public class ModuleDependencyCodeFixProviderTests
{
    [Fact]
    public void CodeFixProvider_IsCreated()
    {
        // Arrange & Act
        var codeFixProvider = new ModuleDependencyCodeFixProvider();

        // Assert
        codeFixProvider.Should().NotBeNull();
    }

    [Fact]
    public void CodeFixProvider_HasCorrectFixableDiagnosticIds()
    {
        // Arrange
        var codeFixProvider = new ModuleDependencyCodeFixProvider();

        // Act
        var fixableDiagnosticIds = codeFixProvider.FixableDiagnosticIds.ToList();

        // Assert
        fixableDiagnosticIds.Should().Contain("CHT001");
        fixableDiagnosticIds.Should().Contain("CHT002");
    }

    [Fact]
    public void CodeFixProvider_ImplementsGetFixAllProvider()
    {
        // Arrange
        var codeFixProvider = new ModuleDependencyCodeFixProvider();

        // Act
        var fixAllProvider = codeFixProvider.GetFixAllProvider();

        // Assert
        fixAllProvider.Should().NotBeNull();
    }
}
