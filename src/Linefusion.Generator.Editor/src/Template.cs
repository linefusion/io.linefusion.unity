using UnityEngine;

using Linefusion.Generator;

namespace Linefusion.Generators
{
    [Icon("Packages/io.linefusion.unity.generator/Content/Icons/Template.png")]
    public class Template : ScriptableObject
    {
        public bool AutoGenerate = false;

        public string Contents = "";

        public TemplateLanguage Language = TemplateLanguage.Default;

        public UnityEngine.Object? Input = null;
    }
}