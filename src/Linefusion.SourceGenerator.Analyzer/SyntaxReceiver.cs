using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace Linefusion.SourceGenerator.Analyzer;

public class SyntaxReceiver : ISyntaxReceiver
{
    public IList<CompilationUnitSyntax> Units { get; private set; } = new List<CompilationUnitSyntax>();
    public IList<TypeDeclarationSyntax> Types { get; private set; } = new List<TypeDeclarationSyntax>();

    public void OnVisitSyntaxNode(SyntaxNode node)
    {
        if (node is CompilationUnitSyntax unit)
        {
            Units.Add(unit);
        }
        else if (node is TypeDeclarationSyntax type)
        {
            Types.Add(type);
        }
    }
}
