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
    public void GenerateMapper_OnRegistryClass_GeneratesIntoRegistryNamespace_UsingConstructor()
    {
        var sourceCode = @"
using System;
using Cheetah.Mapping.Core;

namespace MyApp.Domain { public record User(Guid Id, string Name); }
namespace MyApp.Contracts { public record UserDto(Guid Id, string Name); }

namespace MyApp.Mapping
{
    [GenerateMapper(typeof(MyApp.Domain.User), typeof(MyApp.Contracts.UserDto))]
    public static partial class Registry { }
}
";
        var compilation = CreateCompilation(sourceCode);
        var generator = new MappingGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var generated = driver.GetRunResult().GeneratedTrees[0].ToString();

        // Сгенерировано в неймспейс реестра, НЕ в неймспейс приёмника (Contracts).
        generated.ShouldContain("namespace MyApp.Mapping;");
        generated.ShouldContain("public static class UserToUserDtoMapper");
        generated.ShouldContain("MapToUserDto(this MyApp.Domain.User? source)");
        // record без безпараметрного конструктора → построение через конструктор.
        generated.ShouldContain("new MyApp.Contracts.UserDto(source.Id, source.Name)");

        var compileErrors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        compileErrors.ShouldBeEmpty(string.Join("\n", compileErrors.Select(d => d.ToString())));
    }

    [Fact]
    public void GenerateMapper_AssemblyLevel_GeneratesIntoAssemblyNamespace()
    {
        var sourceCode = @"
using System;
using Cheetah.Mapping.Core;

[assembly: GenerateMapper(typeof(Src), typeof(Dst))]

public record Src(Guid Id, string Name);
public record Dst(Guid Id, string Name);
";
        var compilation = CreateCompilation(sourceCode);
        var generator = new MappingGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var generated = driver.GetRunResult().GeneratedTrees[0].ToString();

        generated.ShouldContain("namespace TestComp;"); // = AssemblyName
        generated.ShouldContain("public static class SrcToDstMapper");
        generated.ShouldContain("new Dst(source.Id, source.Name)");

        var compileErrors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        compileErrors.ShouldBeEmpty(string.Join("\n", compileErrors.Select(d => d.ToString())));
    }

    [Fact]
    public void GenerateMapper_ShouldSkipProjection_WhenGenerateProjectionIsFalse()
    {
        var sourceCode = @"
using System;
using Cheetah.Mapping.Core;

public record Src(Guid Id, string Name);
public record Dst(Guid Id, string Name);

[GenerateMapper(typeof(Src), typeof(Dst), GenerateProjection = false)]
public static partial class Registry { }
";
        var compilation = CreateCompilation(sourceCode);
        var generator = new MappingGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var generated = driver.GetRunResult().GeneratedTrees[0].ToString();

        generated.ShouldContain("MapToDst(this"); // скалярный маппер есть
        generated.ShouldNotContain("ProjectToDst"); // проекции нет

        var compileErrors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        compileErrors.ShouldBeEmpty(string.Join("\n", compileErrors.Select(d => d.ToString())));
    }

    [Fact]
    public void GenerateMapper_ShouldApplyMapMemberRename_FromRegistry()
    {
        var sourceCode = @"
using System;
using Cheetah.Mapping.Core;

// Источник: id из роута называется Id; приёмник ждёт DocumentId.
public record IssueDocumentRequest(Guid Id);
public record IssueDocumentCommand(Guid DocumentId);

[GenerateMapper(typeof(IssueDocumentRequest), typeof(IssueDocumentCommand), GenerateProjection = false)]
[MapMember(typeof(IssueDocumentCommand), ""DocumentId"", ""Id"")]
public static partial class Registry { }
";
        var compilation = CreateCompilation(sourceCode);
        var generator = new MappingGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var generated = driver.GetRunResult().GeneratedTrees[0].ToString();

        // DocumentId конструктора берётся из source.Id — без атрибутов на типах-участниках.
        generated.ShouldContain("new IssueDocumentCommand(source.Id)");

        var compileErrors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        compileErrors.ShouldBeEmpty(string.Join("\n", compileErrors.Select(d => d.ToString())));
    }

    [Fact]
    public void GenerateMapper_ShouldBuildNestedObject_FromMapNested()
    {
        var sourceCode = @"
using System;
using Cheetah.Mapping.Core;

public record AddLineRequest(Guid ProductId, decimal Qty);
public record AddLineToDocumentRequest(Guid Id, Guid ProductId, decimal Qty);
public record AddLineCommand(Guid DocumentId, AddLineRequest Line);

[GenerateMapper(typeof(AddLineToDocumentRequest), typeof(AddLineCommand), GenerateProjection = false)]
[MapMember(typeof(AddLineCommand), ""DocumentId"", ""Id"")]
[MapNested(typeof(AddLineCommand), ""Line"")]
public static partial class Registry { }
";
        var compilation = CreateCompilation(sourceCode);
        var generator = new MappingGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var generated = driver.GetRunResult().GeneratedTrees[0].ToString();

        // DocumentId переименован, Line собран как вложенный объект из плоских полей источника.
        generated.ShouldContain("new AddLineCommand(source.Id, new AddLineRequest(source.ProductId, source.Qty))");

        var compileErrors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        compileErrors.ShouldBeEmpty(string.Join("\n", compileErrors.Select(d => d.ToString())));
    }

    [Fact]
    public void GenerateMapper_ShouldApplyConstant_FromMapConstant()
    {
        var sourceCode = @"
using System;
using Cheetah.Mapping.Core;

public record TokenResult(string Token, int ExpiresInSeconds);
public record TokenViewModel(string AccessToken, string TokenType, int ExpiresIn);

[GenerateMapper(typeof(TokenResult), typeof(TokenViewModel), GenerateProjection = false)]
[MapMember(typeof(TokenViewModel), ""AccessToken"", ""Token"")]
[MapConstant(typeof(TokenViewModel), ""TokenType"", ""Bearer"")]
[MapMember(typeof(TokenViewModel), ""ExpiresIn"", ""ExpiresInSeconds"")]
public static partial class Registry { }
";
        var compilation = CreateCompilation(sourceCode);
        var generator = new MappingGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var generated = driver.GetRunResult().GeneratedTrees[0].ToString();

        // AccessToken и ExpiresIn переименованы, TokenType — константа "Bearer".
        generated.ShouldContain("new TokenViewModel(source.Token, \"Bearer\", source.ExpiresInSeconds)");

        var compileErrors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        compileErrors.ShouldBeEmpty(string.Join("\n", compileErrors.Select(d => d.ToString())));
    }

    [Fact]
    public void GenerateMapper_ShouldMixConstructorAndInitializer()
    {
        var sourceCode = @"
using System;
using Cheetah.Mapping.Core;

public class Source { public Guid Id { get; set; } public string Name { get; set; } = """"; }

public class Dest
{
    public Dest(Guid id) { Id = id; }
    public Guid Id { get; }
    public string Name { get; set; } = """";
}

[GenerateMapper(typeof(Source), typeof(Dest))]
public static partial class Registry { }
";
        var compilation = CreateCompilation(sourceCode);
        var generator = new MappingGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        diagnostics.ShouldBeEmpty();
        var generated = driver.GetRunResult().GeneratedTrees[0].ToString();

        generated.ShouldContain("new Dest(source.Id)"); // конструктор (camelCase параметр → PascalCase свойство)
        generated.ShouldContain("Name = source.Name,"); // остаток через инициализатор

        var compileErrors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        compileErrors.ShouldBeEmpty(string.Join("\n", compileErrors.Select(d => d.ToString())));
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
