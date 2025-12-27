using System.Text;
using Cheetah.Generators.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cheetah.Generators.Application;

[Generator]
public class BootstrapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Фильтруем классы с атрибутом BootstrapperAttribute
        var bootstrapModule = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsClassWithAttribute(node),
                transform: static (context, _) => GetBootstrapClassSymbol(context))
            .Where(static symbol => symbol is not null)
            .Collect();


        // Обрабатываем найденные классы
        context.RegisterSourceOutput(bootstrapModule, static (context, bootstrapModules) =>
        {
            if (bootstrapModules.IsDefaultOrEmpty)
            {
                // Если классов нет, добавляем диагностическое сообщение
                ReportError(context, "Не найден класс с атрибутом BootstrapperAttribute.");
                return;
            }

            if (bootstrapModules.Length > 1)
            {
                // Если классов больше одного, добавляем диагностическое сообщение
                ReportError(context,
                    "Найдено несколько классов с атрибутом BootstrapperAttribute. Ожидается ровно один.");
                return;
            }


            var bootstrapper = bootstrapModules[0]!;

            var dependencies = new Dictionary<INamedTypeSymbol, INamedTypeSymbol[]>(SymbolEqualityComparer.Default);
            FillDependencies(dependencies, bootstrapper);

            if (HasCircularDependency(dependencies, out var cycle))
            {
                ReportError(context, $"Обнаружена циклическая зависимость: {string.Join(" -> ", cycle)}");
                return;
            }

            var sortedModules = TopologicalSort(dependencies, bootstrapper);


            GenerateBootstrapCode(context, bootstrapper);
            GenerateInitializationCode(context, sortedModules, bootstrapper);
        });
    }


    private static List<INamedTypeSymbol> TopologicalSort(Dictionary<INamedTypeSymbol, INamedTypeSymbol[]> graph,
        INamedTypeSymbol startNode)
    {
        var sorted = new List<INamedTypeSymbol>();
        var visited = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        void Visit(INamedTypeSymbol node)
        {
            if (visited.Contains(node))
                return;

            visited.Add(node);

            if (graph.ContainsKey(node))
            {
                foreach (var neighbor in graph[node])
                {
                    Visit(neighbor);
                }
            }

            sorted.Add(node);
        }

        Visit(startNode);
        return sorted;
    }

    private static bool HasCircularDependency(Dictionary<INamedTypeSymbol, INamedTypeSymbol[]> graph,
        out List<string> cycle)
    {
        var visited = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var stack = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var cycles = new List<string>();
        cycle = cycles;

        bool Visit(INamedTypeSymbol node)
        {
            if (stack.Contains(node))
            {
                cycles.Add(node.ToDisplayString());
                return true;
            }

            if (visited.Contains(node))
                return false;

            visited.Add(node);
            stack.Add(node);

            foreach (var neighbor in graph[node])
            {
                if (Visit(neighbor))
                {
                    cycles.Add(node.ToDisplayString());
                    return true;
                }
            }

            stack.Remove(node);
            return false;
        }

        foreach (var node in graph.Keys)
        {
            if (Visit(node))
            {
                cycles.Reverse();
                return true;
            }
        }

        return false;
    }

    private static void FillDependencies(Dictionary<INamedTypeSymbol, INamedTypeSymbol[]> deps, INamedTypeSymbol symbol)
    {
        if (deps.ContainsKey(symbol))
            return;


        var dependencies = GetModuleWithDependencies(symbol);


        deps.Add(dependencies.module, dependencies.dependencies);

        foreach (var dep in dependencies.dependencies)
        {
            FillDependencies(deps, dep);
        }
    }


    private static Dictionary<string, List<string>> BuildDependencyGraph(
        IEnumerable<(INamedTypeSymbol module, List<string> dependencies)?> modules)
    {
        var graph = new Dictionary<string, List<string>>();

        var targetModules = modules.Where(x => x != null)
            .Select(x => x!.Value).ToArray();


        var moduleLookup = targetModules.ToDictionary(m => m.module.ToDisplayString(), m => m.dependencies);

        void AddModuleWithDependencies(string moduleName)
        {
            if (!graph.ContainsKey(moduleName))
            {
                graph[moduleName] = new List<string>();
            }

            if (moduleLookup.TryGetValue(moduleName, out var dependencies))
            {
                foreach (var dependency in dependencies)
                {
                    if (!graph.ContainsKey(dependency))
                    {
                        AddModuleWithDependencies(dependency);
                    }

                    graph[moduleName].Add(dependency);
                }
            }
        }

        foreach (var (module, _) in targetModules)
        {
            AddModuleWithDependencies(module.ToDisplayString());
        }

        return graph;
    }

    private static (INamedTypeSymbol module, INamedTypeSymbol[] dependencies) GetModuleWithDependencies(
        INamedTypeSymbol symbol)
    {
        var dependOnAttributes = symbol.GetAttributes()
            .Where(attr => attr.AttributeClass?.ToDisplayString() == Constants.DependsOnAttributeName)
            .ToList();

        if (!dependOnAttributes.Any())
            return (symbol, Array.Empty<INamedTypeSymbol>());

        var dependencies = dependOnAttributes
            .SelectMany(attr =>
            {
                if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Values != null)
                {
                    return attr.ConstructorArguments[0].Values
                        .Select(arg => arg.Value).OfType<INamedTypeSymbol>();
                }

                return Enumerable.Empty<INamedTypeSymbol>();
            })
            //.Where(dep => !string.IsNullOrEmpty(dep))
            .Distinct(SymbolEqualityComparer.Default)
            .Cast<INamedTypeSymbol>()
            .ToArray();

        return (symbol, dependencies);
    }


    private static (INamedTypeSymbol module, INamedTypeSymbol[] dependencies)? GetModuleWithDependencies(
        GeneratorSyntaxContext context)
    {
        var classDeclaration = (Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
        if (classSymbol is null || !classSymbol.AllInterfaces.Any(i => i.ToDisplayString() == Constants.ModuleTypeName))
            return null;

        return GetModuleWithDependencies(classSymbol);
    }


    private static bool IsClassDeclaration(SyntaxNode node)
    {
        // Проверяем, что узел является определением класса
        return node is ClassDeclarationSyntax classNode &&
               classNode.AttributeLists.Count > 0;
    }


    private static bool IsClassWithAttribute(SyntaxNode node)
    {
        // Проверяем, что узел является определением класса
        return node is ClassDeclarationSyntax classNode &&
               classNode.AttributeLists.Count > 0;
    }

    private static INamedTypeSymbol? GetBootstrapClassSymbol(GeneratorSyntaxContext context)
    {
        // Проверяем, что узел — это класс с атрибутами
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        // Получаем символ класса
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
        if (classSymbol is null)
            return null;

        if (!classSymbol.AllInterfaces.Any(i => i.ToDisplayString() == Constants.ModuleTypeName))
        {
            return null;
        }

        // Проверяем наличие атрибута BootstrapperAttribute
        foreach (var attribute in classSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == Constants.BootstrapperAttributeName)
            {
                return classSymbol;
            }
        }

        return null;
    }

    private static void GenerateBootstrapCode(SourceProductionContext context, INamedTypeSymbol classSymbol)
    {
        // Создаём код для файла Bootstraper.g.cs
        var code = $@"
                using System.Collections.Generic;
                using Microsoft.Extensions.DependencyInjection;
                using Cheetah.Core.Extensions.DependencyInjection;

                namespace {classSymbol.ContainingAssembly.Name}
                {{
                    public static class Bootstrap
                    {{
                        public static void Start(this IServiceCollection services)
                        {{
                            {classSymbol.ContainingAssembly.Name}.ModuleInitializer.InitializeModules();
                            services.AddApplication<{classSymbol.ToDisplayString()}>();
                        }}
                    }}
                }}";

        // Добавляем сгенерированный код
        context.AddSource("Bootstrapper.g.cs", SourceText.From(code, Encoding.UTF8));
    }


    private static void GenerateInitializationCode(SourceProductionContext context,
        List<INamedTypeSymbol> sortedModules, INamedTypeSymbol root)
    {
        var code = new StringBuilder();

        code.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        code.AppendLine();
        code.AppendLine($"namespace {root.ContainingAssembly.Name}");
        code.AppendLine("{");
        code.AppendLine("    public static class ModuleInitializer");
        code.AppendLine("    {");
        code.AppendLine("        public static void InitializeModules()");
        code.AppendLine("        {");

        // code.AppendLine(
        //     $"            Dotnet.Modular.Core.ModuleInitializer.AddModule<Dotnet.Modular.Modules.Core.CoreModule>();");

        foreach (var module in sortedModules)
        {
            code.AppendLine(
                $"            Cheetah.Core.Modularity.ModuleInitializer.AddModule<{module.ToDisplayString()}>();");
        }

        code.AppendLine("        }");
        code.AppendLine("    }");
        code.AppendLine("}");

        context.AddSource("ModuleInitializer.g.cs", SourceText.From(code.ToString(), Encoding.UTF8));
    }

    private static void ReportError(SourceProductionContext context, string message)
    {
        // Создаём диагностическое сообщение об ошибке
        var diagnostic = Diagnostic.Create(new DiagnosticDescriptor(
                id: "BOOTSTRAPGEN001",
                title: "Ошибка генерации Bootstrapper ",
                messageFormat: message,
                category: nameof(BootstrapperGenerator),
                DiagnosticSeverity.Error,
                isEnabledByDefault: true),
            Location.None);

        context.ReportDiagnostic(diagnostic);
    }
}