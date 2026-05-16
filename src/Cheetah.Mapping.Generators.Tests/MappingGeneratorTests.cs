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
        var coreDir = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.IQueryable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Expressions.Expression).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(MapFromAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(System.IO.Path.Combine(coreDir, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(System.IO.Path.Combine(coreDir, "netstandard.dll")),
            MetadataReference.CreateFromFile(System.IO.Path.Combine(coreDir, "System.Collections.dll")),
            MetadataReference.CreateFromFile(System.IO.Path.Combine(coreDir, "System.Linq.dll")),
            MetadataReference.CreateFromFile(System.IO.Path.Combine(coreDir, "System.Linq.Queryable.dll")),
            MetadataReference.CreateFromFile(System.IO.Path.Combine(coreDir, "System.Linq.Expressions.dll")),
        };

        return CSharpCompilation.Create("TestComp",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
    }

    [Fact]
    public void Generator_ShouldGenerateExtensions_WhenValidMapping()
    {
        var sourceCode = @"
using System;
using Cheetah.Mapping.Core;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = """";
    public string DepartmentName { get; set; } = """";
}

[MapFrom(typeof(User))]
public class UserDto
{
    public Guid Id { get; set; }

    [MapProperty(""Name"")]
    public string FullName { get; set; } = """";

    [MapIgnore]
    public string IgnoredField { get; set; } = """";
}
";
        var compilation = CreateCompilation(sourceCode);
        var generator = new MappingGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var runResult = driver.GetRunResult();
        runResult.GeneratedTrees.Length.ShouldBe(1);

        var generatedCode = runResult.GeneratedTrees[0].ToString();
        generatedCode.ShouldContain("public static UserDto? MapToUserDto(this User? source)");
        generatedCode.ShouldContain("Id = source.Id,");
        generatedCode.ShouldContain("FullName = source.Name,");
        generatedCode.ShouldNotContain("IgnoredField");

        // Сгенерированный код должен компилироваться без ошибок.
        var compileErrors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        compileErrors.ShouldBeEmpty(string.Join("\n", compileErrors.Select(d => d.ToString())));
    }

    [Fact]
    public void Generator_ShouldReportDiagnostic_WhenPropertyIsUnmapped()
    {
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
    public string UnknownField { get; set; } = """"; // Unmapped!
}
";
        var compilation = CreateCompilation(sourceCode);
        var generator = new MappingGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        diagnostics.Length.ShouldBe(1);
        var diagnostic = diagnostics[0];
        diagnostic.Id.ShouldBe("CHMAP01");
        diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
        diagnostic.GetMessage().ShouldBe("Property 'UnknownField' in destination 'UserDto' is not mapped to any source property in 'User'. Use [MapIgnore] or [MapProperty] to resolve.");
    }

    [Fact]
    public void Generator_ShouldSupportInitProperties()
    {
        var sourceCode = @"
using System;
using Cheetah.Mapping.Core;

public class User { public Guid Id { get; set; } public string Name { get; set; } = """"; }

[MapFrom(typeof(User))]
public class UserDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
}
";
        var compilation = CreateCompilation(sourceCode);
        var generator = new MappingGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var generated = driver.GetRunResult().GeneratedTrees[0].ToString();
        generated.ShouldContain("Id = source.Id,");
        generated.ShouldContain("Name = source.Name,");
    }

    [Fact]
    public void Generator_ShouldRespectNamespace()
    {
        var sourceCode = @"
using System;
using Cheetah.Mapping.Core;

namespace MyApp.Domain
{
    public class User { public Guid Id { get; set; } }
}

namespace MyApp.Contracts
{
    [MapFrom(typeof(MyApp.Domain.User))]
    public class UserDto { public Guid Id { get; set; } }
}
";
        var compilation = CreateCompilation(sourceCode);
        var generator = new MappingGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var generated = driver.GetRunResult().GeneratedTrees[0].ToString();
        generated.ShouldContain("namespace MyApp.Contracts;");
        generated.ShouldContain("public static class UserDtoMapperExtensions");
    }

    [Fact]
    public void Generator_ShouldHandleTwoClassesWithSameNameInDifferentNamespaces()
    {
        var sourceCode = @"
using System;
using Cheetah.Mapping.Core;

namespace Mod.A
{
    public class User { public Guid Id { get; set; } }
    [MapFrom(typeof(User))]
    public class UserDto { public Guid Id { get; set; } }
}
namespace Mod.B
{
    public class User { public Guid Id { get; set; } }
    [MapFrom(typeof(User))]
    public class UserDto { public Guid Id { get; set; } }
}
";
        var compilation = CreateCompilation(sourceCode);
        var generator = new MappingGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        driver.GetRunResult().GeneratedTrees.Length.ShouldBe(2);

        var compileErrors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        compileErrors.ShouldBeEmpty(string.Join("\n", compileErrors.Select(d => d.ToString())));
    }
}
