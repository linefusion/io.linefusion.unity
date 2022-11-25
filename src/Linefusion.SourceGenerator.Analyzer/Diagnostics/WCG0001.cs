using Microsoft.CodeAnalysis;

namespace Linefusion.SourceGenerator.Analyzer.Diagnostics;

internal static class WCG0001
{
    private const string DiagnosticId = "WCG0001";
    private const string Title = "Invalid template";
    private const string Description = "Make sure the path you are passing to the attribute is relative to this source file.";
    private const string MessageFormat = "The template is invalid";
    private const string Category = "Usage";

    public static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, true, Description);
}
