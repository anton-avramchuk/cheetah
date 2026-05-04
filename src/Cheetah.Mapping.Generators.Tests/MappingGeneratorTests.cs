using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;
using Cheetah.Mapping.Core;
using Cheetah.Mapping.Generators;

namespace Cheetah.Mapping.Generators.Tests;

public class MappingGeneratorTests
{
    private static Compilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(MapFromAttribute).Assembly.Location)
        };

        // Adding standard references for .NET
        var coreDir = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        references.Add(MetadataReference.CreateFromFile(System.IO.Path.Combine(coreDir, "System.Runtime.dll")));

        return CSharpCompilation.Create("TestComp",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    [Fact]
    public void Generator_ShouldGenerateExtensions_WhenValidMapping()
    {
        // Arrange
        var sourceCode = @"
using System;
using Cheetah.Mapping.Core;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string DepartmentName { get; set; }
}

[MapFrom(typeof(User))]
public class UserDto
{
    public Guid Id { get; set; }
    
    [MapProperty(""Name"")]
    public string FullName { get; set; }
    
    [MapIgnore]
    public string IgnoredField { get; set; }
}
";
        var compilation = CreateCompilation(sourceCode);
        var generator = new MappingGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        // Assert
        diagnostics.ShouldBeEmpty();
        var runResult = driver.GetRunResult();
        runResult.GeneratedTrees.Length.ShouldBe(1);
        
        var generatedCode = runResult.GeneratedTrees[0].ToString();
        generatedCode.ShouldContain("public static UserDto MapToUserDto(this User source)");
        generatedCode.ShouldContain("Id = source.Id,");
        generatedCode.ShouldContain("FullName = source.Name,");
        generatedCode.ShouldNotContain("IgnoredField");
    }

    [Fact]
    public void Generator_ShouldReportDiagnostic_WhenPropertyIsUnmapped()
    {
        // Arrange
        var sourceCode = @"
using System;
using Cheetah.Mapping.Core;

public class User
{
    public Guid Id { get; set; }
}

[MapFrom(typeof(User))]
public class UserDto
{
    public Guid Id { get; set; }
    public string UnknownField { get; set; } // Unmapped!
}
";
        var compilation = CreateCompilation(sourceCode);
        var generator = new MappingGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        // Assert
        diagnostics.Length.ShouldBe(1);
        var diagnostic = diagnostics[0];
        diagnostic.Id.ShouldBe("CHMAP01");
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
        diagnostic.GetMessage().ShouldBe("Property 'UnknownField' in destination 'UserDto' is not mapped to any source property in 'User'. Use [MapIgnore] or [MapProperty] to resolve.");
    }
}
