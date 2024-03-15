using UnityEngine;

using Linefusion.Generator;
using System.Collections.Generic;

namespace Linefusion.Generators
{
    [Icon("Packages/io.linefusion.unity.generator/Content/Icons/Template.png")]
    public class Template : ScriptableObject
    {
        public TemplateType Type = TemplateType.Auto;

        public string Contents = "";

        public TemplateLanguage Language = TemplateLanguage.Default;

        public List<UnityEngine.Object> Input = new();
    }
}