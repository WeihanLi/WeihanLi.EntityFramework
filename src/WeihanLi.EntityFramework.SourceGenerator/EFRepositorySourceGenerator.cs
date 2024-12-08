using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace WeihanLi.EntityFramework.SourceGenerator;

[Generator]
public class EFRepositorySourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var additionalFiles = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith(".template"))
            .Select((file, cancellationToken) => file.GetText(cancellationToken)?.ToString())
            .Where(content => !string.IsNullOrEmpty(content));

        var dbContextDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsDbContextDeclaration(s),
                transform: static (ctx, _) => GetDbContextDeclaration(ctx))
            .Where(static m => m is not null);

        var combined = dbContextDeclarations.Combine(additionalFiles.Collect());

        context.RegisterSourceOutput(combined, (spc, source) =>
        {
            var (dbContextDeclaration, templates) = source;
            var dbContextName = dbContextDeclaration!.Identifier.Text;
            var repositoryNamespace = dbContextDeclaration!.Parent!.GetNamespace();
            var generatedCode = GenerateRepositoryCode(dbContextName, repositoryNamespace, templates);
            spc.AddSource($"{dbContextName}Repository.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
        });
    }

    private static bool IsDbContextDeclaration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDeclaration &&
               classDeclaration.BaseList?.Types.Any(t => t.ToString().Contains("DbContext")) == true;
    }

    private static ClassDeclarationSyntax? GetDbContextDeclaration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        return classDeclaration;
    }

    private static string GenerateRepositoryCode(string dbContextName, string repositoryNamespace, ImmutableArray<string> templates)
    {
        var builder = new StringBuilder();
        builder.AppendLine("using WeihanLi.EntityFramework;");
        builder.AppendLine($"namespace {repositoryNamespace}");
        builder.AppendLine("{");

        foreach (var template in templates)
        {
            var code = template.Replace("{{DbContextName}}", dbContextName);
            builder.AppendLine(code);
        }

        builder.AppendLine("}");
        return builder.ToString();
    }
}

internal static class SyntaxNodeExtensions
{
    public static string GetNamespace(this SyntaxNode node)
    {
        while (node != null)
        {
            if (node is NamespaceDeclarationSyntax namespaceDeclaration)
            {
                return namespaceDeclaration.Name.ToString();
            }

            node = node.Parent;
        }

        return string.Empty;
    }
}
