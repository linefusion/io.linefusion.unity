using System.IO;
using System.Threading.Tasks;

using Linefusion.Generator.IO;
using Linefusion.Generators;
using Linefusion.Generators.Functions;

using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Linefusion.Generators.Editor
{
    public class TemplateLoader : ITemplateLoader
    {
        private readonly PathValue root;

        public TemplateLoader(string root)
        {
            this.root = root;
        }

        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        {
            var workingDir = new PathValue(context.CurrentSourceFile).Parent;

            var templatePath = new PathValue(templateName);
            if (templatePath.IsAbsolute)
            {
                return templatePath;
            }

            if (templatePath.IsRooted)
            {
                using var path = PathUtils.UsePath(UnityUtils.ProjectDir);
                templatePath = templatePath.Absolute;
            }
            else
            {
                using var path = PathUtils.UsePath(workingDir);
                templatePath = templatePath.Absolute;
            }

            return templatePath;
        }

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            return File.ReadAllText(templatePath);
        }

        public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            return new ValueTask<string>(Task.FromResult(Load(context, callerSpan, templatePath)));
        }
    }
}