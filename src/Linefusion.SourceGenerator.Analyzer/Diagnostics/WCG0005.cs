using Microsoft.CodeAnalysis;

namespace Linefusion.SourceGenerator.Analyzer.Diagnostics;

internal static class WCG0005
{
    private const string DiagnosticId = "WCG0005";
    private const string Title = "Template generated warning";
    private const string Description = "Template generated an warning.";
    private const string MessageFormat = "Template generated an warning";
    private const string Category = "Usage";

    public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description);
}
