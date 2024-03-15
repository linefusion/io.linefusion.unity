using System;
using System.Collections.Generic;
using System.IO;
using Linefusion.Generator;
using Scriban.Parsing;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Linefusion.Generators.Editor
{
    public class BaseTemplateImporter : ScriptedImporter
    {
        private readonly TemplateLanguage language = TemplateLanguage.Default;

        public TemplateType Type = TemplateType.Auto;
        public List<UnityEngine.Object> Input = new List<UnityEngine.Object>();

        public BaseTemplateImporter(TemplateLanguage language)
        {
            this.language = language;
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var icon = Content.Load<Texture2D>("Icons/Template.png");

            var assetPath = ctx.assetPath.Clone() as string;

            var tpl = ScriptableObject.CreateInstance<Template>();
            tpl.Contents = File.ReadAllText(ctx.assetPath);
            tpl.Language = language;

            if (Type == TemplateType.Auto)
            {
                tpl.Type = tpl.Contents.Contains("file.write")
                    ? TemplateType.Generator
                    : TemplateType.Include;
            }
            else
            {
                tpl.Type = Type;
            }

            tpl.Input = this.Input;

            ctx.AddObjectToAsset("template", tpl, icon);
            ctx.SetMainObject(tpl);

            MonoScript script = MonoScript.FromScriptableObject(tpl);
            var scriptIcon = EditorGUIUtility.GetIconForObject(script);
            if (scriptIcon == null || scriptIcon != icon)
            {
                EditorGUIUtility.SetIconForObject(script, icon);
            }

            if (tpl.Type.HasFlag(TemplateType.Include))
            {
                Dispatcher.Enqueue(() =>
                {
                    TemplateGenerator.Generate(false);
                }, 1);
            }
            else if (tpl.Type.HasFlag(TemplateType.Generator))
            {
                Dispatcher.Enqueue(() =>
                {
                    TemplateGenerator.Generate(assetPath!, false);
                }, 1);
            }
        }

        [MenuItem("Assets/Create/Linefusion/Generators/Scriban Generator", false)]
        private static void CreateScribanGenerator()
        {
            ProjectWindowUtil.CreateAssetWithContent(
                "NewGenerator.sbncs",
                @"Hello {{ data | json }}"
            );
        }

        [MenuItem("Assets/Create/Linefusion/Generators/Liquid Generator", false)]
        private static void CreateLiquidGenerator()
        {
            ProjectWindowUtil.CreateAssetWithContent(
                "NewGenerator.cs.liquid",
                @"Hello {{ data | json }}"
            );
        }
    }

    [ScriptedImporter(1, new string[] { "liquid", })]
    class LiquidImporter : BaseTemplateImporter
    {
        public LiquidImporter()
            : base(TemplateLanguage.Liquid) { }
    }

    [ScriptedImporter(
        1,
        new string[]
        {
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
        }
    )]
    class ScribanImporter : BaseTemplateImporter
    {
        public ScribanImporter()
            : base(TemplateLanguage.Scriban) { }
    }
}
