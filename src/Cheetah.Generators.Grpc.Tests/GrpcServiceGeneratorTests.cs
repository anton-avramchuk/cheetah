using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace Cheetah.Generators.Grpc.Tests;

public class GrpcServiceGeneratorTests
{
    // Фейковые proto-типы (минимальный IMessage), proto-база (как у Grpc.Tools) и CQRS-типы.
    private const string Preamble = @"
using System;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Backend.Grpc.Abstractions;
using Cheetah.Core.CQRS;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Grpc.Core;

namespace Test.App
{
    public abstract class FakeMessage : IMessage
    {
        public MessageDescriptor Descriptor => null!;
        public int CalculateSize() => 0;
        public void MergeFrom(CodedInputStream input) { }
        public void WriteTo(CodedOutputStream output) { }
    }

    public sealed class HelloRequest : FakeMessage { public string Id { get; set; } = """"; }
    public sealed class HelloReply : FakeMessage { public string Text { get; set; } = """"; }
    public sealed class OrphanRequest : FakeMessage { public string Id { get; set; } = """"; }

    // Имитация сгенерированного Grpc.Tools базового класса сервиса.
    public static class Greeter
    {
        public abstract class GreeterBase
        {
            public virtual Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
                => throw new NotImplementedException();
        }
    }

    public sealed record GetHelloQuery(string Id) : IQuery<string>;
    public sealed record GetHelloOrNullQuery(string Id) : IQuery<string?>;
    public sealed record SayHelloCommand(string Id) : ICommand<string>;
    public sealed record DoThingCommand(string Id) : ICommand;
}
";

    private sealed record RunResult(string Generated, ImmutableArrayWrapper Diagnostics, int TreeCount, IReadOnlyList<string> CompileErrors);

    // упрощённая обёртка чтобы не тянуть using ImmutableArray в сигнатуры
    private sealed class ImmutableArrayWrapper
    {
        private readonly IReadOnlyList<Diagnostic> _items;
        public ImmutableArrayWrapper(IReadOnlyList<Diagnostic> items) => _items = items;
        public int Length => _items.Count;
        public IEnumerable<Diagnostic> Items => _items;
    }

    private static RunResult Run(string markers)
    {
        var source = Preamble + markers;
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var coreDir = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(global::Cheetah.Backend.Grpc.Abstractions.GrpcCommand<,>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(global::Cheetah.Core.CQRS.ICommand).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(global::Cheetah.Mapping.Core.IObjectMapper).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(global::Grpc.Core.ServerCallContext).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(global::Google.Protobuf.IMessage).Assembly.Location),
            MetadataReference.CreateFromFile(System.IO.Path.Combine(coreDir, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(System.IO.Path.Combine(coreDir, "netstandard.dll")),
            MetadataReference.CreateFromFile(System.IO.Path.Combine(coreDir, "System.Collections.dll")),
            MetadataReference.CreateFromFile(System.IO.Path.Combine(coreDir, "System.Threading.Tasks.dll")),
        };

        var compilation = CSharpCompilation.Create("TestComp",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        var generator = new GrpcServiceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        var runResult = driver.GetRunResult();
        var generated = string.Join("\n", runResult.GeneratedTrees.Select(t => t.ToString()));

        var compileErrors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => d.ToString())
            .ToList();

        return new RunResult(generated, new ImmutableArrayWrapper(diagnostics), runResult.GeneratedTrees.Length, compileErrors);
    }

    private const string QueryMarker =
        "namespace Test.App { public sealed class GetHelloGrpc : GrpcQuery<HelloRequest, GetHelloQuery, string, HelloReply> { } }";

    private const string QueryOrNotFoundMarker =
        "namespace Test.App { public sealed class GetHelloGrpc : GrpcQueryOrNotFound<HelloRequest, GetHelloOrNullQuery, string?, HelloReply> { } }";

    private const string CommandWithResultMarker =
        "namespace Test.App { public sealed class SayHelloGrpc : GrpcCommandWithResult<HelloRequest, SayHelloCommand, string, HelloReply> { } }";

    private const string CommandMarker =
        "namespace Test.App { public sealed class DoThingGrpc : GrpcCommand<HelloRequest, DoThingCommand> { } }";

    [Fact]
    public void Query_GeneratesServiceDerivingFromProtoBase_WithDispatcherCall()
    {
        var r = Run(QueryMarker);

        r.TreeCount.ShouldBe(1);
        r.Generated.ShouldContain("class GreeterService : global::Test.App.Greeter.GreeterBase");
        r.Generated.ShouldContain("public override async global::System.Threading.Tasks.Task<global::Test.App.HelloReply> SayHello(");
        r.Generated.ShouldContain("global::Test.App.HelloRequest request, global::Grpc.Core.ServerCallContext context");
        r.Generated.ShouldContain("_mapper.Map<global::Test.App.GetHelloQuery>(request)");
        r.Generated.ShouldContain("_dispatcher.QueryAsync<global::Test.App.GetHelloQuery,");
        r.Generated.ShouldContain("_mapper.Map<global::Test.App.HelloReply>(result)");
        r.Generated.ShouldNotContain("StatusCode.NotFound");
        r.CompileErrors.ShouldBeEmpty(string.Join("\n", r.CompileErrors));
    }

    [Fact]
    public void QueryOrNotFound_GeneratesNullCheckThrowingNotFound()
    {
        var r = Run(QueryOrNotFoundMarker);

        r.TreeCount.ShouldBe(1);
        r.Generated.ShouldContain("if (result is null)");
        r.Generated.ShouldContain("global::Grpc.Core.StatusCode.NotFound");
        r.Generated.ShouldContain("new global::Grpc.Core.RpcException");
        r.CompileErrors.ShouldBeEmpty(string.Join("\n", r.CompileErrors));
    }

    [Fact]
    public void CommandWithResult_GeneratesSendAsyncAndMapsResult()
    {
        var r = Run(CommandWithResultMarker);

        r.TreeCount.ShouldBe(1);
        r.Generated.ShouldContain("_mapper.Map<global::Test.App.SayHelloCommand>(request)");
        r.Generated.ShouldContain("_dispatcher.SendAsync<global::Test.App.SayHelloCommand,");
        r.Generated.ShouldContain("_mapper.Map<global::Test.App.HelloReply>(result)");
        r.CompileErrors.ShouldBeEmpty(string.Join("\n", r.CompileErrors));
    }

    [Fact]
    public void Command_Void_GeneratesSendAsyncAndReturnsNewResponse()
    {
        var r = Run(CommandMarker);

        r.TreeCount.ShouldBe(1);
        r.Generated.ShouldContain("_mapper.Map<global::Test.App.DoThingCommand>(request)");
        r.Generated.ShouldContain("await _dispatcher.SendAsync(command, context.CancellationToken)");
        r.Generated.ShouldContain("return new global::Test.App.HelloReply();");
        r.CompileErrors.ShouldBeEmpty(string.Join("\n", r.CompileErrors));
    }

    [Fact]
    public void UnmatchedProtoRequest_ReportsDiagnostic_AndGeneratesNothing()
    {
        var marker =
            "namespace Test.App { public sealed class OrphanGrpc : GrpcQuery<OrphanRequest, GetHelloQuery, string, HelloReply> { } }";

        var r = Run(marker);

        r.TreeCount.ShouldBe(0);
        r.Diagnostics.Items.ShouldContain(d => d.Id == "GRPCGEN001");
    }

    [Fact]
    public void NoMarkers_GeneratesNothing()
    {
        var r = Run(string.Empty);

        r.TreeCount.ShouldBe(0);
    }

    [Fact]
    public void MultipleMarkers_SameService_GenerateSingleServiceClass()
    {
        // два метода в одной proto-базе -> один сервис-класс с двумя override
        var multiBase = @"
namespace Test.App
{
    public sealed class CatGetRequest : FakeMessage { public string Id { get; set; } = """"; }
    public sealed class CatGetReply : FakeMessage { public string Text { get; set; } = """"; }
    public sealed class CreateRequest : FakeMessage { public string Name { get; set; } = """"; }
    public sealed class CreateReply : FakeMessage { public string Id { get; set; } = """"; }

    public static class Catalog
    {
        public abstract class CatalogBase
        {
            public virtual System.Threading.Tasks.Task<CatGetReply> Get(CatGetRequest request, Grpc.Core.ServerCallContext context)
                => throw new System.NotImplementedException();
            public virtual System.Threading.Tasks.Task<CreateReply> Create(CreateRequest request, Grpc.Core.ServerCallContext context)
                => throw new System.NotImplementedException();
        }
    }

    public sealed record CatGetQuery(string Id) : Cheetah.Core.CQRS.IQuery<string>;
    public sealed record CreateThingCommand(string Name) : Cheetah.Core.CQRS.ICommand<string>;

    public sealed class GetGrpc : GrpcQuery<CatGetRequest, CatGetQuery, string, CatGetReply> { }
    public sealed class CreateGrpc : GrpcCommandWithResult<CreateRequest, CreateThingCommand, string, CreateReply> { }
}";

        var r = Run(multiBase);

        r.TreeCount.ShouldBe(1);
        r.Generated.ShouldContain("class CatalogService : global::Test.App.Catalog.CatalogBase");
        r.Generated.ShouldContain("Get(");
        r.Generated.ShouldContain("Create(");
        r.CompileErrors.ShouldBeEmpty(string.Join("\n", r.CompileErrors));
    }
}
