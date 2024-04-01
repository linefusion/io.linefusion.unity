using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Linefusion.Generator.IO;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using UnityEditor;

using UnityEngine;

namespace Linefusion.Generators.Editor
{
    public class TemplateLoader : ITemplateLoader
    {
        private readonly SafePath root;

        public TemplateLoader(string root)
        {
            this.root = root;
        }

        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        {
            SafePath? templatePath = null;

            if (Uri.TryCreate(templateName, UriKind.Absolute, out var uri))
            {
                var workingDir = new SafePath(context.CurrentSourceFile).Parent;
                if (uri.Scheme == "std")
                {
                    var templates = UnityUtils.GetPackagePath("Templates");
                    var safeTemplates = SafeIO.Resolve(templates);

                    using var root = SafeIO.UsePath(templates);

                    templatePath = TemplateExtensions
                        .Extensions.Select(ext => Path.Combine(safeTemplates, $"{uri.AbsolutePath}.{ext}"))
                        .FirstOrDefault(SafeIO.Exists);
                }
                else
                {
                    throw new Exception($"Invalid template context: {uri.Scheme}");
                }
            }
            else
            {
                var workingDir = new SafePath(context.CurrentSourceFile).Parent;

                templatePath = new SafePath(templateName);
                if (templatePath.IsAbsolute)
                {
                    return templatePath;
                }

                if (templatePath.IsRooted)
                {
                    using var path = SafeIO.UsePath(UnityUtils.ProjectDir);
                    templatePath = templatePath.Absolute;
                }
                else
                {
                    using var path = SafeIO.UsePath(workingDir);
                    templatePath = templatePath.Absolute;
                }
            }

            if (templatePath == null)
            {
                throw new Exception($"Template not found: {templateName}");
            }

            return templatePath;
        }

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            return File.ReadAllText(templatePath);
        }

        public ValueTask<string> LoadAsync(
            TemplateContext context,
            SourceSpan callerSpan,
            string templatePath
        )
        {
            return new ValueTask<string>(Task.FromResult(Load(context, callerSpan, templatePath)));
        }
    }
}
