using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.IO;

using Linefusion.Generator;
using Scriban.Parsing;

namespace Linefusion.Generators.Editor
{
    public class BaseTemplateImporter : ScriptedImporter
    {
        private readonly TemplateLanguage language = TemplateLanguage.Default;

        public bool AutoGenerate = false;
        public Object? Input = null;

        public BaseTemplateImporter(TemplateLanguage language)
        {
            this.language = language;
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var icon = Content.Load<Texture2D>("Icons/Template.png");

            var tpl = ScriptableObject.CreateInstance<Template>();
            tpl.Contents = File.ReadAllText(ctx.assetPath);
            tpl.Language = language;
            tpl.AutoGenerate = this.AutoGenerate;
            tpl.Input = this.Input;

            ctx.AddObjectToAsset("template", tpl, icon);
            ctx.SetMainObject(tpl);

            MonoScript script = MonoScript.FromScriptableObject(tpl);
            var scriptIcon = EditorGUIUtility.GetIconForObject(script);
            if (scriptIcon == null || scriptIcon != icon)
            {
                EditorGUIUtility.SetIconForObject(script, icon);
            }

            if (tpl.AutoGenerate)
            {
                TemplateGenerator.Generate(tpl, ctx.assetPath);
            }
        }

        [MenuItem("Assets/Create/Linefusion/Generators/Scriban Generator", false)]
        private static void CreateScribanGenerator()
        {
            ProjectWindowUtil.CreateAssetWithContent(
                "NewGenerator.generator.scriban",
                @"Hello {{ data | json }}"
            );
        }
        
        [MenuItem("Assets/Create/Linefusion/Generators/Liquid Generator", false)]
        private static void CreateLiquidGenerator()
        {
            ProjectWindowUtil.CreateAssetWithContent(
                "NewGenerator,generator.liquid",
                @"Hello {{ data | json }}"
            );
        }
    }


    [ScriptedImporter(1, new string[] {
        "liquid",
    })]
    class LiquidImporter : BaseTemplateImporter
    {
        public LiquidImporter() :
            base(TemplateLanguage.Liquid)
        {
        }
    }

    [ScriptedImporter(1, new string[] {
        "scriban",
        "scriban-cs",
        "scriban-txt",
        "scriban-htm",
        "scriban-html",
        "sbn-cs",
        "sbn-txt",
        "sbn-htm",
        "sbn-html",
        "sbn",
        "sbncs",
        "sbntxt",
        "sbnhtm",
        "sbnhtml",
    })]
    class ScribanImporter : BaseTemplateImporter
    {
        public ScribanImporter() :
            base(TemplateLanguage.Scriban)
        {
        }
    }
}