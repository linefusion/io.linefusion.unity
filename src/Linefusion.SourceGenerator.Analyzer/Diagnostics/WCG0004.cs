using Microsoft.CodeAnalysis;

namespace Linefusion.SourceGenerator.Analyzer.Diagnostics;

internal static class WCG0004
{
    private const string DiagnosticId = "WCG0004";
    private const string Title = "Template generated error";
    private const string Description = "Template generated an error.";
    private const string MessageFormat = "Template generated an error";
    private const string Category = "Usage";

    public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description);
}
