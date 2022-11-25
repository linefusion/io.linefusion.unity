using System;
using System.Collections.Generic;
using System.Text;

using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Linefusion.SourceGenerator.Analyzer.Functions;

internal class ScriptFileFunctions : ScriptObject
{
    public static void Write(TemplateContext context, string path, string contents)
    {
        var root = Template.Evaluate("dir", context).ToString();
        var filePath = Path.Combine(root, path);

        var dirPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        if (File.Exists(filePath))
        {
            var previous = File.ReadAllText(filePath);
            if (previous == contents)
            {
                return;
            }
        }
        
        File.WriteAllText(filePath, contents);
    }
}
