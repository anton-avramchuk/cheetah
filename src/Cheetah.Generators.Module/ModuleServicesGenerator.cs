using System.Text;
using Cheetah.Core.DependencyInjection;
using Cheetah.Generators.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cheetah.Generators.Module;

[Generator]
public class ModuleServicesGenerator : IIncrementalGenerator
{
    private const string ErrorCode = "MODGEN001";
    private static string ErrorCategory = nameof(ModuleServicesGenerator);
    private const string IdempotentAttributeName = "Cheetah.Core.Outbox.IdempotentAttribute";
    private const string EventHandlerInterfacePrefix = "Cheetah.Core.Events.IEventHandler<";
    private const string IdempotentDecoratorTypeName = "Cheetah.Core.Outbox.InboxIdempotentEventHandler";
    private const string InboxStoreTypeName = "Cheetah.Core.Outbox.IInboxStore";
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Находим все классы, реализующие IModule
        var modules = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsClassDeclaration(node),
                transform: static (context, _) => GetModuleIfImplementsIModule(context))
            .Where(static module => module is not null)
            .Collect();

        context.RegisterSourceOutput(modules, static (ctx, moduleSymbols) =>
        {
            if (moduleSymbols.IsDefaultOrEmpty)
            {
                ctx.AddError(ErrorCode, "Не найдено классов, реализующих IModule.", ErrorCategory);
                return;
            }

            if (moduleSymbols.Length > 1)
            {
                ctx.AddError(ErrorCode, "Найдено несколько классов, реализующих IModule. Ожидается ровно один.", ErrorCategory);
                return;
            }

            var moduleSymbol = moduleSymbols[0]!;

            if (!moduleSymbol.IsPartial())
            {
                ctx.AddError(ErrorCode, $"Класс {moduleSymbol.Name} должен быть partial.", ErrorCategory);
                return;
            }

            GenerateModuleCode(ctx, moduleSymbol);
        });
    }

    private static void GenerateModuleCode(SourceProductionContext ctx, INamedTypeSymbol moduleSymbol)
    {



        var servicesRegistration = GenerateServicesRegistration(moduleSymbol);

        var code = $@"
using Microsoft.Extensions.DependencyInjection;

namespace {moduleSymbol.ContainingNamespace}
{{
    public partial class {moduleSymbol.Name}
    {{
        public void RegisterServices(IServiceCollection services)
        {{
            {servicesRegistration}
        }}
    }}
}}";

        ctx.AddSource($"{moduleSymbol.Name}_Generated.g.cs", SourceText.From(code, Encoding.UTF8));




    }


    private static string GenerateServicesRegistration(INamedTypeSymbol moduleSymbol)
    {
        var compilation = moduleSymbol.ContainingAssembly;
        var classesWithExport = compilation.GetTypesWithAttribute(Constants.ExportAttributeName!);
        var registrations = new StringBuilder();

        foreach (var classSymbol in classesWithExport)
        {
            if(classSymbol.IsAbstract)
                continue;

            var isIdempotent = classSymbol.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString() == IdempotentAttributeName);

            foreach (var attribute in classSymbol.GetAttributes().Where(a => a.AttributeClass?.ToDisplayString() == Constants.ExportAttributeName))
            {
                var exportType = (LifetimeType)attribute.ConstructorArguments[0].Value!;
                var exportedTypes = attribute.ConstructorArguments[1].Values;
                var key = attribute.NamedArguments.FirstOrDefault(a => a.Key == "Key").Value.Value?.ToString();

                string implementationType = GetTypeName(classSymbol);

                if (key != null)
                {
                    // Keyed service registration
                    var keyedMethod = exportType switch
                    {
                        LifetimeType.Scoped => "AddKeyedScoped",
                        LifetimeType.Transient => "AddKeyedTransient",
                        LifetimeType.Singleton => "AddKeyedSingleton",
                        _ => throw new NotImplementedException(),
                    };

                    var escapedKey = key.Replace("\\", "\\\\").Replace("\"", "\\\"");

                    if (exportedTypes.Length == 0)
                    {
                        registrations.AppendLine($"services.{keyedMethod}(typeof({implementationType}), \"{escapedKey}\", typeof({implementationType}));");
                    }
                    else
                    {
                        foreach (var exportedType in exportedTypes)
                        {
                            var serviceType = exportedType.Value?.ToString();
                            var resolvedServiceType = !string.IsNullOrEmpty(serviceType) ? serviceType : implementationType;
                            registrations.AppendLine($"services.{keyedMethod}(typeof({resolvedServiceType}), \"{escapedKey}\", typeof({implementationType}));");
                        }
                    }
                }
                else
                {
                    // Standard (non-keyed) registration
                    var registrationMethod = exportType switch
                    {
                        LifetimeType.Scoped => "AddScoped",
                        LifetimeType.Transient => "AddTransient",
                        LifetimeType.Singleton => "AddSingleton",
                        _ => throw new NotImplementedException(),
                    };

                    if (exportedTypes.Length == 0)
                    {
                        registrations.AppendLine($"services.{registrationMethod}(typeof({implementationType}));");
                    }
                    else if (exportedTypes.Length == 1)
                    {
                        var serviceType = exportedTypes[0].Value?.ToString();
                        if (!string.IsNullOrEmpty(serviceType))
                        {
                            if (isIdempotent && IsEventHandlerInterface(serviceType, out var eventType))
                            {
                                AppendIdempotentRegistration(registrations, registrationMethod, implementationType, eventType);
                            }
                            else
                            {
                                registrations.AppendLine($"services.{registrationMethod}(typeof({serviceType}), typeof({implementationType}));");
                            }
                        }
                        else
                        {
                            registrations.AppendLine($"services.{registrationMethod}(typeof({implementationType}));");
                        }
                    }
                    else
                    {
                        // Multiple interfaces: register concrete type once, bridge each interface to it
                        registrations.AppendLine($"services.{registrationMethod}(typeof({implementationType}));");
                        foreach (var exportedType in exportedTypes)
                        {
                            var serviceType = exportedType.Value?.ToString();
                            if (string.IsNullOrEmpty(serviceType))
                                continue;

                            if (isIdempotent && IsEventHandlerInterface(serviceType, out var eventType))
                            {
                                AppendIdempotentBridge(registrations, registrationMethod, implementationType, eventType);
                            }
                            else
                            {
                                registrations.AppendLine($"services.{registrationMethod}(typeof({serviceType}), sp => sp.GetRequiredService(typeof({implementationType})));");
                            }
                        }
                    }
                }
            }
        }

        return registrations.ToString();
    }

    /// <summary>
    /// Проверяет, является ли строка вида "Cheetah.Core.Events.IEventHandler&lt;TEvent&gt;"
    /// и извлекает TEvent.
    /// </summary>
    private static bool IsEventHandlerInterface(string serviceTypeName, out string eventType)
    {
        eventType = string.Empty;
        if (!serviceTypeName.StartsWith(EventHandlerInterfacePrefix) || !serviceTypeName.EndsWith(">"))
            return false;

        eventType = serviceTypeName.Substring(
            EventHandlerInterfacePrefix.Length,
            serviceTypeName.Length - EventHandlerInterfacePrefix.Length - 1);
        return eventType.Length > 0;
    }

    private static void AppendIdempotentRegistration(StringBuilder sb, string registrationMethod, string implementationType, string eventType)
    {
        // Регистрируем сам хендлер, чтобы декоратор мог его получить
        sb.AppendLine($"services.{registrationMethod}(typeof({implementationType}));");
        sb.AppendLine(
            $"services.{registrationMethod}<global::{EventHandlerInterfacePrefix.TrimEnd('<')}<global::{eventType}>>(sp => " +
            $"new global::{IdempotentDecoratorTypeName}<global::{eventType}>(" +
            $"(global::{EventHandlerInterfacePrefix.TrimEnd('<')}<global::{eventType}>)sp.GetRequiredService(typeof({implementationType})), " +
            $"sp.GetRequiredService<global::{InboxStoreTypeName}>(), " +
            $"sp.GetRequiredService<global::Microsoft.Extensions.Logging.ILogger<global::{IdempotentDecoratorTypeName}<global::{eventType}>>>()));");
    }

    private static void AppendIdempotentBridge(StringBuilder sb, string registrationMethod, string implementationType, string eventType)
    {
        // Конкретный тип уже зарегистрирован выше. Только bridge на IEventHandler через декоратор.
        sb.AppendLine(
            $"services.{registrationMethod}<global::{EventHandlerInterfacePrefix.TrimEnd('<')}<global::{eventType}>>(sp => " +
            $"new global::{IdempotentDecoratorTypeName}<global::{eventType}>(" +
            $"(global::{EventHandlerInterfacePrefix.TrimEnd('<')}<global::{eventType}>)sp.GetRequiredService(typeof({implementationType})), " +
            $"sp.GetRequiredService<global::{InboxStoreTypeName}>(), " +
            $"sp.GetRequiredService<global::Microsoft.Extensions.Logging.ILogger<global::{IdempotentDecoratorTypeName}<global::{eventType}>>>()));");
    }

    /// <summary>
    /// Генерирует строковое представление типа, включая поддержку дженериков.
    /// </summary>
    private static string GetTypeName(INamedTypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeParameters.Length == 0)
        {
            return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        }

        string baseName = typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? typeSymbol.Name
            : $"{typeSymbol.ContainingNamespace}.{typeSymbol.Name}";

        string genericParams = new string(',', typeSymbol.TypeParameters.Length - 1);

        return $"{baseName}<{genericParams}>";
    }






    private static bool IsClassDeclaration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax;
    }

    private static INamedTypeSymbol? GetModuleIfImplementsIModule(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol is null)
            return null;
        
        if (classSymbol.IsAbstract)
            return null;

        if (!classSymbol.AllInterfaces.Any(i => i.ToDisplayString() == Constants.ModuleTypeName))
            return null;

        return classSymbol;
    }
}