using System;
using System.Collections.Generic;
using System.Text;

using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Linefusion.SourceGenerator.Analyzer.Scripts;

internal class ScriptLoader : ITemplateLoader
{
    private readonly string _root;

    public ScriptLoader(string root)
    {
        _root = root;
    }

    public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
    {
        return Path.Combine(_root, templateName);
    }

    public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        return Parse(File.ReadAllText(templatePath));
    }

    public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        return new ValueTask<string>(Parse(File.ReadAllText(templatePath)));
    }

    public static string Parse(string input)
    {
        var content = input.Split('\n').ToList();
        if (content.Count > 0)
        {
            var hasCheck = false;

            for (var i = 0; i < content.Count; i++)
            {
                var line = content[i].Trim();
                if (line == "")
                {
                    continue;
                }
                else if (line == "#if INSIDE_GENERATOR" || line.StartsWith("#if INSIDE_GENERATOR"))
                {
                    hasCheck = true;
                    content[i] = "// " + content[i];
                    break;
                }
                else if (line != "")
                {
                    break;
                }
            }

            if (hasCheck)
            {
                for (var i = content.Count - 1; i >= 0; i--)
                {
                    if (content[i].Trim() == "#endif" || content[i].Trim().StartsWith("#endif"))
                    {
                        content[i] = "// " + content[i];
                        break;
                    }
                }
            }
        }
        return string.Join("\n", content);
    }
}
