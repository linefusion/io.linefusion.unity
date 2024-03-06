using Linefusion.Generator.IO;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Scriban;
using Scriban.Runtime;

namespace Linefusion.Generator
{
    public class TemplateRenderer
    {
        private string root;
        private ITemplateLoader? loader;

        public TemplateRenderer(string root, ITemplateLoader? loader)
        {
            this.root = root;
            this.loader = loader;
        }

        public object Render(TemplateLanguage language, string source, string path, ScriptObject[] objs)
        {
            path = new PathValue(path).Absolute;

            var lang = Scriban.Parsing.ScriptLang.Liquid;
            if (language == TemplateLanguage.Scriban)
            {
                lang = Scriban.Parsing.ScriptLang.Default;
            }

            var parserOptions = new Scriban.Parsing.ParserOptions()
            {
                ExpressionDepthLimit = 1000000,
                LiquidFunctionsToScriban = true,
                ParseFloatAsDecimal = false,
            };

            var lexerOptions = new Scriban.Parsing.LexerOptions()
            {
                FrontMatterMarker = "---",
                KeepTrivia = true,
                Lang = lang,
                Mode = Scriban.Parsing.ScriptMode.Default,
                StartPosition = new Scriban.Parsing.TextPosition(0, 0, 0),
            };

            var template = Template.Parse(source, path, parserOptions, lexerOptions);

            var context = new TemplateContext()
            {
                TemplateLoader = loader,
                LoopLimit = 1000000,
                LoopLimitQueryable = 1000000,
            };
            
            context.PushSourceFile(path);

            foreach (var obj in objs)
            {
                context.PushGlobal(obj);
            }

            return template.Render(context);
        }
    }
}
