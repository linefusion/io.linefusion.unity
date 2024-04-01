using System;
using System.IO;
using System.Reflection;
using Linefusion.Generator;
using Linefusion.Generator.IO;
using Linefusion.Generators.Functions;
using Microsoft.CodeAnalysis;
using NTypewriter.Runtime.CodeModel;
using Scriban.Runtime;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Linefusion.Generators.Editor
{
    public class TemplateGenerator
    {
        [MenuItem("Linefusion/Generators/Generate All", false)]
        public static void Generate()
        {
            Generate(true);
        }

        public static void Generate(bool import)
        {
            try
            {
                var assets = AssetDatabase.FindAssets("t:" + typeof(Template).FullName);
                foreach (var asset in assets)
                {
                    Generate(AssetDatabase.GUIDToAssetPath(asset), import);
                }

                if (import)
                {
                    FileFunctions.Reimport();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void Generate(string path, bool import = true)
        {
            if (import)
            {
                AssetDatabase.ImportAsset(path);
            }

            var template = AssetDatabase.LoadAssetAtPath<Template>(path);
            if (template != null && template.Type.HasFlag(TemplateType.Generator))
            {
                Generate(template, path);
            }
        }

        public static void Generate(Template? template, string path)
        {
            var root = new SafePath(Path.Combine(Application.dataPath, "..")).Absolute;
            var file = new SafePath(Path.Combine(root, path)).Absolute;

            try
            {
                var globals = TemplateObject2.Create();

                globals.Import(
                    "typeof",
                    (object v) =>
                        v switch
                        {
                            Type t => t,
                            _ => v.GetType()
                        }
                );

                globals.Import(
                    "typestr",
                    new Func<object, object>(
                        (object v) =>
                            v switch
                            {
                                Type t => t.FullName,
                                string s => s,
                                _ => v.GetType().FullName
                            }
                    )
                );

                globals.Import("instanceof", (object v, Type t) => t.IsAssignableFrom(v.GetType()));

                var exports = TemplateObject2
                    .Create()
                    .Set<UnityFunctions>("unity")
                    .Set<JsonFunctions>("json")
                    .Set<FileFunctions>("file")
                    .Set<DirFunctions>("directory")
                    .Set<CsharpFunctions>("csharp")
                    .Set<PathFunctions>("paths")
                    .Set(
                        "dotnet",
                        TemplateObject2
                            .Create()
                            .Set<ReflectionFunctions>("reflection")
                            .Set("type", TemplateObject2.Wrap(Type.GetType("System.Type")))
                            .Set(
                                "member_info",
                                TemplateObject2.Wrap(Type.GetType("System.Reflection.MemberInfo"))
                            )
                            .Merge(globals)
                    )
                    .Set("input", template?.Input, true)
                    .Set(
                        "context",
                        TemplateObject2
                            .Create()
                            .Set("filename", file, true)
                            .Set("directory", Path.GetDirectoryName(file), true)
                    );

                var renderer = new TemplateRenderer(root, new TemplateLoader(root));
                var result = renderer.Render(
                    template?.Language ?? TemplateLanguage.Scriban,
                    template?.Contents ?? "",
                    file,
                    new[] { exports, globals }
                );
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void Flush()
        {
            FileFunctions.Reimport();
        }
    }
}
