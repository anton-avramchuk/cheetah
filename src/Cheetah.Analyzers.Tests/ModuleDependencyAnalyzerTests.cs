namespace Cheetah.Analyzers.Tests;

public class ModuleDependencyAnalyzerTests
{
    [Fact]
    public void Analyzer_IsCreated()
    {
        // Arrange & Act
        var analyzer = new ModuleDependencyAnalyzer();

        // Assert
        analyzer.ShouldNotBeNull();
        analyzer.SupportedDiagnostics.Length.ShouldBe(2);
    }

    [Fact]
    public void Analyzer_HasCorrectDiagnosticIds()
    {
        // Arrange
        var analyzer = new ModuleDependencyAnalyzer();

        // Act
        var diagnosticIds = analyzer.SupportedDiagnostics.Select(d => d.Id).ToList();

        // Assert
        diagnosticIds.ShouldContain("CHT001");
        diagnosticIds.ShouldContain("CHT002");
    }

    [Fact]
    public void MissingDependency_HasErrorSeverity()
    {
        // Arrange
        var analyzer = new ModuleDependencyAnalyzer();

        // Act
        var missingDependencyDiagnostic = analyzer.SupportedDiagnostics.First(d => d.Id == "CHT001");

        // Assert
        missingDependencyDiagnostic.DefaultSeverity.ShouldBe(Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
    }

    [Fact]
    public void UnusedDependency_HasWarningSeverity()
    {
        // Arrange
        var analyzer = new ModuleDependencyAnalyzer();

        // Act
        var unusedDependencyDiagnostic = analyzer.SupportedDiagnostics.First(d => d.Id == "CHT002");

        // Assert
        unusedDependencyDiagnostic.DefaultSeverity.ShouldBe(Microsoft.CodeAnalysis.DiagnosticSeverity.Warning);
    }
}
