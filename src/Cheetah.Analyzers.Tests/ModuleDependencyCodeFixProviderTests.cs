namespace Cheetah.Analyzers.Tests;

public class ModuleDependencyCodeFixProviderTests
{
    [Fact]
    public void CodeFixProvider_IsCreated()
    {
        // Arrange & Act
        var codeFixProvider = new ModuleDependencyCodeFixProvider();

        // Assert
        codeFixProvider.ShouldNotBeNull();
    }

    [Fact]
    public void CodeFixProvider_HasCorrectFixableDiagnosticIds()
    {
        // Arrange
        var codeFixProvider = new ModuleDependencyCodeFixProvider();

        // Act
        var fixableDiagnosticIds = codeFixProvider.FixableDiagnosticIds.ToList();

        // Assert
        fixableDiagnosticIds.ShouldContain("CHT001");
        fixableDiagnosticIds.ShouldContain("CHT002");
    }

    [Fact]
    public void CodeFixProvider_ImplementsGetFixAllProvider()
    {
        // Arrange
        var codeFixProvider = new ModuleDependencyCodeFixProvider();

        // Act
        var fixAllProvider = codeFixProvider.GetFixAllProvider();

        // Assert
        fixAllProvider.ShouldNotBeNull();
    }
}
