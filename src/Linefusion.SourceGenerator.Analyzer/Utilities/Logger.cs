using System;
using System.Collections.Generic;
using System.Text;

namespace Linefusion.SourceGenerator.Analyzer.Utilities;

internal class Logger
{
    private readonly List<string> _lines = new();

    public void Log(IEnumerable<string> lines)
    {
#if DEBUG
        foreach (var line in lines)
        {
            Log(line);
        }
#endif
    }

    public void Log(params string[] lines)
    {
#if DEBUG
        foreach (var line in lines)
        {
            Log(line);
        }
#endif
    }

    public void Log(string line)
    {
#if DEBUG
        _lines.AddRange(line.Split('\n'));
#endif
    }

    public override string ToString()
    {
        return string.Join("\n", _lines);
    }

    public string[] ToArray()
    {
        return _lines.ToArray();
    }
}

