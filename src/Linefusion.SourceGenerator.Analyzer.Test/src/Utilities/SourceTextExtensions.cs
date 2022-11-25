using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Text;

namespace Linefusion.SourceGenerator.Analyzer.Test.Utilities;

public static class SourceTextExtensions
{
    public static string? Source(this SourceText text)
    {
        var type = text.GetType();
        if (type == null)
        {
            return null;
        }

        var source = type.GetProperty("Source");
        if (source == null)
        {
            return null;
        }

        return (string?)source.GetValue(text);
    }
}
