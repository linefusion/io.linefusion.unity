using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.IO;
using Linefusion.Generator;
using System;
using Scriban.Runtime;
using Linefusion.Generators.Functions;
using Linefusion.Generator.IO;
using System.Reflection;
using NTypewriter.Runtime.CodeModel;
using Microsoft.CodeAnalysis;


namespace Linefusion.Generators.Editor
{
    public class TemplateGenerator
    {
        [MenuItem("Linefusion/Generators/Generate All", false)]
        public static void Generate()
        {
            var assets = AssetDatabase.FindAssets("t:" + typeof(Template).FullName);
            try {
                    
                AssetDatabase.StartAssetEditing();

                foreach (var asset in assets)
                {
                    var path = AssetDatabase.GUIDToAssetPath(asset);
                    var template = AssetDatabase.LoadAssetAtPath<Template>(path);
                    if (template != null)
                    {
                        Generate(template, path);
                    }
                }
                
            } catch (Exception e) {
                Debug.LogException(e);
            } finally {
                AssetDatabase.StopAssetEditing();
            }
        }

        public static void Generate(Template? template, string path)
        {
            var root = new PathValue(Path.Combine(Application.dataPath, "..")).Absolute;
            var file = new PathValue(Path.Combine(root, path)).Absolute;

            try
            {

                var globals = TemplateObject2.Create();
                
                globals.Import("typeof", (object v) => v switch {
                    Type t => t,
                    _ => v.GetType()
                });

                globals.Import("typestr", new Func<object, object>((object v) => v switch {
                    Type t => t.FullName,
                    string s => s,
                    _ => v.GetType().FullName
                }));

                globals.Import("instanceof", (object v, Type t) => t.IsAssignableFrom(v.GetType())); 

                var exports = TemplateObject2
                    .Create()
                    .Set<UnityFunctions>("unity")
                    .Set<JsonFunctions>("json")
                    .Set<FileFunctions>("file")
                    .Set<CsharpFunctions>("csharp")
                    .Set("dotnet", TemplateObject2.Create()
                        .Set<ReflectionFunctions>("reflection")
                        .Set("type", TemplateObject2.Wrap(Type.GetType("System.Type")))
                        .Set("member_info", TemplateObject2.Wrap(Type.GetType("System.Reflection.MemberInfo")))
                        .Merge(globals)
                    )
                    .Set("input", template?.Input, true)
                    .Set("context",
                        TemplateObject2.Create()
                            .Set("filename", file, true)
                            .Set("directory", Path.GetDirectoryName(file), true)
                );              

                var renderer = new TemplateRenderer(root, new TemplateLoader(root));
                var result = renderer.Render(template?.Language ?? TemplateLanguage.Scriban, template?.Contents ?? "", file, new[] {
                    exports, globals
                });
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

}