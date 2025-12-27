using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cheetah.Generators.Module
{
    public static class SymbolExtensions
    {
        public static bool IsPartial(this INamedTypeSymbol symbol)
        {
            return symbol.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>()
                .Any(c => c.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)));
        }

        public static ImmutableArray<INamedTypeSymbol> GetTypesWithAttribute(this IAssemblySymbol assembly, string attributeName)
        {
            var results = ImmutableArray.CreateBuilder<INamedTypeSymbol>();

            // Рекурсивная функция для обхода пространства имен
            void SearchNamespace(INamespaceSymbol namespaceSymbol)
            {
                // Ищем типы в текущем пространстве имен
                foreach (var type in namespaceSymbol.GetTypeMembers())
                {
                    if (type.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == attributeName))
                    {
                        results.Add(type);
                    }

                    // Рекурсивно обходим вложенные типы
                    foreach (var nestedType in type.GetTypeMembers())
                    {
                        if (nestedType.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == attributeName))
                        {
                            results.Add(nestedType);
                        }
                    }
                }

                // Рекурсивно обходим вложенные пространства имен
                foreach (var nestedNamespace in namespaceSymbol.GetNamespaceMembers())
                {
                    SearchNamespace(nestedNamespace);
                }
            }

            // Запускаем поиск с глобального пространства имен
            SearchNamespace(assembly.GlobalNamespace);

            return results.ToImmutable();
        }
    }

    public static class SourceProductionContextExtensions
    {
        public static void AddError(this SourceProductionContext context, string code, string message, string category)
        {
            var diagnostic = Diagnostic.Create(new DiagnosticDescriptor(
                id: code,
                title: message,
                messageFormat: message,
                category: category,
                DiagnosticSeverity.Error,
                isEnabledByDefault: true),
                Location.None);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
