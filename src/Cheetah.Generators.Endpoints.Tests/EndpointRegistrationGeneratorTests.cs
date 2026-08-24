using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace Cheetah.Generators.Endpoints.Tests;

/// <summary>
/// Генератор регистрации эндпоинтов: по классу-метаданным он пишет `Map*`, разбор запроса, вызов
/// диспетчера и ответ.
///
/// Проверяется именно сгенерированный текст, потому что ошибка здесь не даёт ошибки сборки: новая
/// база, не попавшая в список поиска, компилируется молча — а маршрута в приложении просто нет.
/// Ровно так уже исчезал DELETE-с-результатом, и поймалось это сверкой спеки, а не тестом.
/// </summary>
public class EndpointRegistrationGeneratorTests
{
    /// <summary>Модуль (генератор группирует эндпоинты по нему), CQRS-типы, запросы и ответы.</summary>
    private const string Preamble = @"
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Microsoft.AspNetCore.Http;

namespace Test.App
{
    public partial class TestModule : CrmModule
    {
    }

    public sealed record UploadFileRequest(IFormFile File, string? Source) : ICrmRequest;
    public sealed record ImportResultResponse(int Created) : ICrmResponse;
    public sealed record ParseFileCommand(string FileName) : ICommand<int>;
}
";

    private const string UploadMarker = @"
namespace Test.App
{
    public sealed class ParseFileEndpoint
        : UploadCommandEndpoint<UploadFileRequest, ParseFileCommand, int, ImportResultResponse>
    {
        public override string Route => ""/api/import/preview"";
    }
}
";

    /// <summary>Загрузка файла: форма вместо тела JSON, и всё остальное — как у обычной команды.</summary>
    [Fact]
    public void Upload_endpoint_binds_the_form_and_dispatches_the_command()
    {
        var generated = Run(UploadMarker);

        generated.ShouldContain("routeBuilder.MapPost(endpoint.Route");
        generated.ShouldContain("[FromForm] Test.App.UploadFileRequest request");
        generated.ShouldContain("mapper.Map<Test.App.ParseFileCommand>(request)");
        generated.ShouldContain("dispatcher.SendAsync<Test.App.ParseFileCommand, int>");
        generated.ShouldContain("mapper.Map<Test.App.ImportResultResponse>(result)");
        generated.ShouldContain("Results.Ok(response)");
    }

    /// <summary>
    /// Без <c>DisableAntiforgery</c> маршрут с формой отвечает 500 ещё до обработчика: метаданные
    /// требуют middleware защиты от подделки, которого в приложении может не быть вовсе.
    /// </summary>
    [Fact]
    public void Upload_endpoint_disables_antiforgery()
    {
        Run(UploadMarker).ShouldContain("builder.DisableAntiforgery();");
    }

    /// <summary>
    /// Тип содержимого объявлен явно. Иначе ASP.NET выводит его из <c>[FromForm]</c> и пишет в
    /// спеку ДВА варианта — multipart и x-www-form-urlencoded, — хотя файл вторым не передать.
    /// Генератор клиента честно делает по функции на вариант, и у загрузки появляется двойник,
    /// который отвечает 415.
    /// </summary>
    [Fact]
    public void Upload_endpoint_accepts_multipart_only()
    {
        var generated = Run(UploadMarker);

        generated.ShouldContain("builder.Accepts<Test.App.UploadFileRequest>(\"multipart/form-data\");");
        generated.ShouldNotContain("x-www-form-urlencoded");
    }

    /// <summary>
    /// Ответ и «плохой запрос» объявлены: испорченный файл — это 400, и клиент разбирает его как
    /// ответ, а не как неизвестную ошибку.
    /// </summary>
    [Fact]
    public void Upload_endpoint_declares_its_responses()
    {
        var generated = Run(UploadMarker);

        generated.ShouldContain("builder.Produces<Test.App.ImportResultResponse>(StatusCodes.Status200OK);");
        generated.ShouldContain("builder.Produces(StatusCodes.Status400BadRequest);");
    }

    /// <summary>
    /// Права, объявленные через <c>RequirePermissions</c>, обязаны превращаться в политики.
    /// Раньше генератор их не читал вовсе: вызов компилировался, эндпоинт оставался открытым,
    /// и ни ошибки, ни предупреждения — при том что README рекламирует это как способ защиты.
    /// </summary>
    [Fact]
    public void Endpoint_applies_required_permissions_as_policies()
    {
        var generated = Run(UploadMarker);

        generated.ShouldContain("foreach (var permission in endpoint.RequiredPermissions)");
        generated.ShouldContain("global::Cheetah.Core.Authorization.PermissionPolicy.For(permission)");
    }

    private static string Run(string marker)
    {
        var source = Preamble + marker;
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var coreDir = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(global::Cheetah.Backend.Endpoints.EndpointBase<,>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(global::Cheetah.Contracts.Requests.ICrmRequest).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(global::Cheetah.Core.CQRS.ICommand).Assembly.Location),
            // Через CrmModuleDescriptor, а не ICrmModule: последний вкомпилирован и в сборку самого
            // генератора (он линкует его исходником), и имя разрешалось бы в два разных типа.
            MetadataReference.CreateFromFile(typeof(global::Cheetah.Core.Modularity.CrmModuleDescriptor).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(global::Microsoft.AspNetCore.Http.IFormFile).Assembly.Location),
            MetadataReference.CreateFromFile(System.IO.Path.Combine(coreDir, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(System.IO.Path.Combine(coreDir, "netstandard.dll")),
            MetadataReference.CreateFromFile(System.IO.Path.Combine(coreDir, "System.Collections.dll")),
        };

        var compilation = CSharpCompilation.Create("TestComp",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new EndpointRegistrationGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        return string.Join("\n", driver.GetRunResult().GeneratedTrees.Select(tree => tree.ToString()));
    }
}
