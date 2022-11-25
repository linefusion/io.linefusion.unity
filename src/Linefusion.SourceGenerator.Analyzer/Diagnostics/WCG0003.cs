using Microsoft.CodeAnalysis;

namespace Linefusion.SourceGenerator.Analyzer.Diagnostics;

internal static class WCG0003
{
    private const string DiagnosticId = "WCG0003";
    private const string Title = "Template has errors";
    private const string Description = "Found errors when parsing template file.";
    private const string MessageFormat = "Template parse failed";
    private const string Category = "Usage";

    public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description);
}
