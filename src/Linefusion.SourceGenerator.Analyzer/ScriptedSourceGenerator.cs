using System.Security.Cryptography;
using System.Text;

using Linefusion.SourceGenerator.Analyzer.Diagnostics;
using Linefusion.SourceGenerator.Analyzer.Extensions;
using Linefusion.SourceGenerator.Analyzer.Functions;
using Linefusion.SourceGenerator.Analyzer.Scripts;
using Linefusion.SourceGenerator.Analyzer.Utilities;
using Linefusion.SourceGenerator.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;

namespace Linefusion.SourceGenerator.Analyzer;

[Generator]
public class ScriptedSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext generatorContext)
    {
        if (generatorContext.SyntaxReceiver is not SyntaxReceiver data)
        {
            return;
        }
        
#if DEBUG
        var output_dir = "E:\\wolfulus\\codegen\\output\\" + generatorContext.Compilation.Assembly.Name;
        Directory.CreateDirectory(output_dir);
#endif

        var exceptions = new List<Exception>();
        
        var log = new Logger();
        log.Log("This Assembly: " + System.Reflection.Assembly.GetExecutingAssembly().Location);

        try
        {
            var templates = new List<Template>();
            foreach (var unit in data.Units)
            {
                log.Log($"Processing unit {unit.SyntaxTree.FilePath}");
                if (unit.AttributeLists.Count <= 0)
                {
                    continue;
                }
                
                log.Log($"> Found {unit.AttributeLists.Count} attribute lists");
                foreach (var list in unit.AttributeLists)
                {
                    foreach (var attr in list.Attributes)
                    {
                        // Invalid argument count
                        if (attr.ArgumentList == null)
                        {
                            log.Log($" > Skipping attribute with null argumentlist");
                            continue;
                        }

                        if (attr.ArgumentList.Arguments.Count <= 0)
                        {
                            log.Log($" > Skipping attribute with no arguments");
                            continue;
                        }

                        if (!(attr.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax pathLiteral))
                        {
                            log.Log($" > Skipping attribute that first argument isn't a string literal");
                            continue;
                        }

                        var model = generatorContext.Compilation.GetSemanticModel(unit.SyntaxTree);

                        var type = model.GetTypeInfo(attr);
                        if (type.Type == null)
                        {
                            log.Log($" > Skipping attribute semantic type is null");
                            continue;
                        }

                        if (type.Type.GetFullName() != "Linefusion.SourceGenerator.Abstractions.Attributes.UseGenerator")
                        {
                            log.Log($" > Skipping attribute that has a different type: {type.Type.GetFullName()}");
                            continue;
                        }

                        var name = type.Type.Name;
                        var path = pathLiteral.Token.ValueText;
                        var templateDir = Path.GetDirectoryName(attr.SyntaxTree.FilePath);
                        var templateFile = Path.GetFullPath(Path.Combine(templateDir, path));

                        if (!File.Exists(templateFile))
                        {
                            log.Log($" > Specified template file doesn't exist: {templateFile}");
                            generatorContext.ReportDiagnostic(Diagnostic.Create(WCG0001.Descriptor, Location.Create(attr.SyntaxTree, attr.ArgumentList.Arguments[0].Span), $"Template file found: {templateFile}"));
                        }
                        else
                        {
                            try
                            {
                                var template = Template.Parse(ScriptLoader.Parse(File.ReadAllText(templateFile)), templateFile);
                                if (template.HasErrors)
                                {
                                    log.Log($" > Template has errors: {templateFile}");
                                    foreach (var error in template.Messages)
                                    {
                                        log.Log($"  > {error}");
                                        generatorContext.ReportDiagnostic(Diagnostic.Create(WCG0003.Descriptor, Location.Create(attr.SyntaxTree, attr.ArgumentList.Arguments[0].Span), error.Message));
                                        exceptions.Add(new Exception(error.Message));
                                    }
                                    continue;
                                }
                                
                                templates.Add(template);
                                log.Log($" > Template added");
                            }
                            catch (Exception e)
                            {
                                generatorContext.ReportDiagnostic(Diagnostic.Create(WCG0002.Descriptor, Location.Create(attr.SyntaxTree, attr.ArgumentList.Arguments[0].Span), e.Message));
                                log.Log($" > Template Error: {e.Message}");
                            }
                        }
                    }
                }
            }

            Context ctx = new(generatorContext.Compilation);
            
            log.Log($"> Generating {templates.Count} templates");

            var iteration = 0;

            foreach (var template in templates)
            {
                iteration++;

                var hasher = SHA256.Create();
                var hash = hasher.ComputeHash(Encoding.ASCII.GetBytes(iteration.ToString() + "|" + template.SourceFilePath));
                var filename = Path.GetFileNameWithoutExtension(template.SourceFilePath) + "_" + BitConverter.ToString(hash).Replace("-", "") + ".g.cs";
                var folder = Path.GetDirectoryName(template.SourceFilePath);

                var context = new TemplateContext { TemplateLoader = new ScriptLoader(folder) };
                
                context.PushGlobal(new ScriptObject() {
                    ["dir"] = folder,
                    ["assembly"] = ctx.Assembly,
                    ["file"] = new ScriptFileFunctions(),
                });
                
                var output = template.Render(context);
                
                log.Log($"> Adding file {filename}");
                generatorContext.AddSource(filename, SourceText.From(output, Encoding.UTF8));
                
#if DEBUG
                FileWriter.Write($"{output_dir}\\{filename}", output);
#endif
            }
        }
        catch (Exception e)
        {
            log.Log($"> General Error {e.Message}");
            exceptions.Add(e);
        }

        log.Log($"Finished");
        
#if DEBUG
        FileWriter.Write($"{output_dir}\\errors.txt", string.Join("\n\n", exceptions.Select(e => e.Message + "\n" + e.StackTrace)));
        FileWriter.Write($"{output_dir}\\debug.txt", log.ToString());
#endif
        generatorContext.AddSource("Readme.g.cs", SourceText.From("// Files generated by Linefusion.SourceGenerator.Analyzer\n// https://assetstore.unity.com/...", Encoding.UTF8, SourceHashAlgorithm.Sha256));
    }
}
