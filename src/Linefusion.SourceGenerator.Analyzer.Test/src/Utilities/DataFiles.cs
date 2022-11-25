using System.IO;
using System.Reflection;
using System.Text;

using Microsoft.CodeAnalysis.Text;

namespace Linefusion.SourceGenerator.Analyzer.Test.Utilities;

public static class DataFiles
{
    public static (string, SourceText) Read(string name)
    {
        var path = Path.GetFullPath($"data\\{name}");
        var source = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, File.ReadAllBytes(path)));
        return (path, SourceText.From(source, Encoding.UTF8, SourceHashAlgorithm.Sha256));
    }

    public static (string, SourceText)[] ReadMany(params string[] names)
    {
        var files = new (string, SourceText)[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            files[i] = Read(names[i]);
        }
        return files;
    }

    /*
    public static (string, SourceText) ReadWithName(string name)
    {
        var (path, content) = Read(name);
        
        var header = "// filename: ";
        if (content.StartsWith(header))
        {
            var lines = content.Split('\n').ToList();

            // 1st line
            name = lines[0].Substring(header.Length).Trim();
            lines.RemoveAt(0);

            if (lines.Count > 0)
            {
                if (lines[0].Trim() == "")
                {
                    lines.RemoveAt(0);
                }
            }
            content = String.Join('\n', lines);
        }

        return (path, SourceText.From(content, Encoding.UTF8, SourceHashAlgorithm.Sha256));
    }

    public static (string, SourceText)[] ReadManyWithName(params string[] names)
    {
        var files = new (string, SourceText)[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            files[i] = ReadWithName(names[i]);
        }
        return files;
    }
    */
}