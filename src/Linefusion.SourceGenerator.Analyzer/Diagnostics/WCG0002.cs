using Microsoft.CodeAnalysis;

namespace Linefusion.SourceGenerator.Analyzer.Diagnostics;

internal static class WCG0002
{
    private const string DiagnosticId = "WCG0002";
    private const string Title = "Failed to parse template";
    private const string Description = "There was an error while parsing the template file.";
    private const string MessageFormat = "Template failed";
    private const string Category = "Usage";

    public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description);
}
